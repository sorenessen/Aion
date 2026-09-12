import { describe, expect, it } from 'vitest'

import {
  defaultPresentationPolicy,
  selectPresentationState,
  type PresentationState,
} from './presentation-policy'

const hidden: PresentationState = {
  localStructuresVisible: false,
}

const visible: PresentationState = {
  localStructuresVisible: true,
}

const relevantScreenSignificance = 0.05

describe('selectPresentationState', () => {
  it('keeps local structures hidden above the entry threshold', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters: 5_000,
          localRepresentationScreenSignificance:
            relevantScreenSignificance,
        },
        hidden,
      ),
    ).toEqual(hidden)
  })

  it('enters local representation below the entry threshold when it contributes to the view', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters:
            defaultPresentationPolicy.localStructuresEnterHeightMeters - 1,
          localRepresentationScreenSignificance:
            relevantScreenSignificance,
        },
        hidden,
      ),
    ).toEqual(visible)
  })

  it('does not enter local representation when screen significance is zero', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters:
            defaultPresentationPolicy.localStructuresEnterHeightMeters - 1,
          localRepresentationScreenSignificance: 0,
        },
        hidden,
      ),
    ).toEqual(hidden)
  })

  it('keeps local structures visible through the height hysteresis band while they contribute to the view', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters: 2_500,
          localRepresentationScreenSignificance:
            relevantScreenSignificance,
        },
        visible,
      ),
    ).toEqual(visible)
  })

  it('keeps hidden structures hidden through the height hysteresis band', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters: 2_500,
          localRepresentationScreenSignificance:
            relevantScreenSignificance,
        },
        hidden,
      ),
    ).toEqual(hidden)
  })

  it('leaves local representation above the exit threshold', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters:
            defaultPresentationPolicy.localStructuresExitHeightMeters + 1,
          localRepresentationScreenSignificance:
            relevantScreenSignificance,
        },
        visible,
      ),
    ).toEqual(hidden)
  })

  it('leaves local representation when screen significance becomes zero', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters: 1_000,
          localRepresentationScreenSignificance: 0,
        },
        visible,
      ),
    ).toEqual(hidden)
  })

  it('does not let screen significance override the height entry boundary', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters: 5_000,
          localRepresentationScreenSignificance: 1,
        },
        hidden,
      ),
    ).toEqual(hidden)
  })

  it('rejects non-finite camera height measurements', () => {
    expect(() =>
      selectPresentationState(
        {
          cameraHeightMeters: Number.NaN,
          localRepresentationScreenSignificance:
            relevantScreenSignificance,
        },
        hidden,
      ),
    ).toThrow('cameraHeightMeters')
  })

  it('accepts finite below-reference camera height measurements', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters: -500,
          localRepresentationScreenSignificance:
            relevantScreenSignificance,
        },
        hidden,
      ),
    ).toEqual(visible)
  })

  it('accepts normalized local representation screen significance', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters: 5_000,
          localRepresentationScreenSignificance: 1,
        },
        hidden,
      ),
    ).toEqual(hidden)
  })

  it('rejects invalid local representation screen significance', () => {
    for (const significance of [
      Number.NaN,
      Number.POSITIVE_INFINITY,
      -0.01,
      1.01,
    ]) {
      expect(() =>
        selectPresentationState(
          {
            cameraHeightMeters: 1_000,
            localRepresentationScreenSignificance: significance,
          },
          hidden,
        ),
      ).toThrow('localRepresentationScreenSignificance')
    }
  })

  it('rejects overlapping or inverted hysteresis thresholds', () => {
    expect(() =>
      selectPresentationState(
        {
          cameraHeightMeters: 1_000,
          localRepresentationScreenSignificance:
            relevantScreenSignificance,
        },
        hidden,
        {
          localStructuresEnterHeightMeters: 3_000,
          localStructuresExitHeightMeters: 2_000,
        },
      ),
    ).toThrow('thresholds')
  })
})

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

describe('selectPresentationState', () => {
  it('keeps local structures hidden above the entry threshold', () => {
    expect(
      selectPresentationState(
        { cameraHeightMeters: 5_000 },
        hidden,
      ),
    ).toEqual(hidden)
  })

  it('enters local representation below the entry threshold', () => {
    expect(
      selectPresentationState(
        {
          cameraHeightMeters:
            defaultPresentationPolicy.localStructuresEnterHeightMeters - 1,
        },
        hidden,
      ),
    ).toEqual(visible)
  })

  it('keeps local structures visible through the hysteresis band', () => {
    expect(
      selectPresentationState(
        { cameraHeightMeters: 2_500 },
        visible,
      ),
    ).toEqual(visible)
  })

  it('keeps hidden structures hidden through the hysteresis band', () => {
    expect(
      selectPresentationState(
        { cameraHeightMeters: 2_500 },
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
        },
        visible,
      ),
    ).toEqual(hidden)
  })

  it('rejects invalid view measurements', () => {
    expect(() =>
      selectPresentationState(
        { cameraHeightMeters: Number.NaN },
        hidden,
      ),
    ).toThrow('cameraHeightMeters')

    expect(() =>
      selectPresentationState(
        { cameraHeightMeters: -1 },
        hidden,
      ),
    ).toThrow('cameraHeightMeters')
  })

  it('rejects overlapping or inverted hysteresis thresholds', () => {
    expect(() =>
      selectPresentationState(
        { cameraHeightMeters: 1_000 },
        hidden,
        {
          localStructuresEnterHeightMeters: 3_000,
          localStructuresExitHeightMeters: 2_000,
        },
      ),
    ).toThrow('thresholds')
  })
})

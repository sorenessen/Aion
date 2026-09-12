import { describe, expect, it } from 'vitest'

import {
  calculateScreenRectangleUnionCoverage,
} from './screen-coverage'

describe('calculateScreenRectangleUnionCoverage', () => {
  it('returns zero for no rectangles', () => {
    expect(
      calculateScreenRectangleUnionCoverage([], 100, 100),
    ).toBe(0)
  })

  it('measures one rectangle against the viewport', () => {
    expect(
      calculateScreenRectangleUnionCoverage(
        [
          {
            minX: 0,
            minY: 0,
            maxX: 50,
            maxY: 50,
          },
        ],
        100,
        100,
      ),
    ).toBeCloseTo(0.25)
  })

  it('does not double count overlapping rectangles', () => {
    expect(
      calculateScreenRectangleUnionCoverage(
        [
          {
            minX: 0,
            minY: 0,
            maxX: 60,
            maxY: 60,
          },
          {
            minX: 40,
            minY: 40,
            maxX: 100,
            maxY: 100,
          },
        ],
        100,
        100,
      ),
    ).toBeCloseTo(0.68)
  })

  it('does not double count a contained rectangle', () => {
    expect(
      calculateScreenRectangleUnionCoverage(
        [
          {
            minX: 10,
            minY: 10,
            maxX: 90,
            maxY: 90,
          },
          {
            minX: 30,
            minY: 30,
            maxX: 70,
            maxY: 70,
          },
        ],
        100,
        100,
      ),
    ).toBeCloseTo(0.64)
  })

  it('measures disjoint rectangles independently', () => {
    expect(
      calculateScreenRectangleUnionCoverage(
        [
          {
            minX: 0,
            minY: 0,
            maxX: 25,
            maxY: 100,
          },
          {
            minX: 75,
            minY: 0,
            maxX: 100,
            maxY: 100,
          },
        ],
        100,
        100,
      ),
    ).toBeCloseTo(0.5)
  })

  it('returns exactly one when rectangles cover the full viewport', () => {
    expect(
      calculateScreenRectangleUnionCoverage(
        [
          {
            minX: 0,
            minY: 0,
            maxX: 100,
            maxY: 100,
          },
        ],
        100,
        100,
      ),
    ).toBe(1)
  })

  it('keeps fractional full-viewport coverage inside the normalized range', () => {
    const coverage =
      calculateScreenRectangleUnionCoverage(
        [
          {
            minX: 0,
            minY: 0,
            maxX: 33.333333333333336,
            maxY: 73.33333333333333,
          },
          {
            minX: 33.333333333333336,
            minY: 0,
            maxX: 66.66666666666667,
            maxY: 73.33333333333333,
          },
          {
            minX: 66.66666666666667,
            minY: 0,
            maxX: 100,
            maxY: 73.33333333333333,
          },
          {
            minX: 0,
            minY: 73.33333333333333,
            maxX: 100,
            maxY: 100,
          },
        ],
        100,
        100,
      )

    expect(coverage).toBeGreaterThanOrEqual(0)
    expect(coverage).toBeLessThanOrEqual(1)
    expect(coverage).toBeCloseTo(1)
  })

  it('rejects invalid viewport dimensions', () => {
    expect(() =>
      calculateScreenRectangleUnionCoverage([], 0, 100),
    ).toThrow('Viewport dimensions')

    expect(() =>
      calculateScreenRectangleUnionCoverage([], 100, Number.NaN),
    ).toThrow('Viewport dimensions')
  })

  it('rejects rectangles outside the viewport', () => {
    expect(() =>
      calculateScreenRectangleUnionCoverage(
        [
          {
            minX: -1,
            minY: 0,
            maxX: 50,
            maxY: 50,
          },
        ],
        100,
        100,
      ),
    ).toThrow('Screen rectangles')
  })

  it('rejects empty or inverted rectangles', () => {
    expect(() =>
      calculateScreenRectangleUnionCoverage(
        [
          {
            minX: 50,
            minY: 10,
            maxX: 50,
            maxY: 90,
          },
        ],
        100,
        100,
      ),
    ).toThrow('Screen rectangles')
  })
})

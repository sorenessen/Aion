export interface ScreenRectangle {
  minX: number
  minY: number
  maxX: number
  maxY: number
}

export function calculateScreenRectangleUnionCoverage(
  rectangles: readonly ScreenRectangle[],
  viewportWidth: number,
  viewportHeight: number,
): number {
  if (
    !Number.isFinite(viewportWidth)
    || !Number.isFinite(viewportHeight)
    || viewportWidth <= 0
    || viewportHeight <= 0
  ) {
    throw new Error(
      'Viewport dimensions must be finite positive numbers.',
    )
  }

  for (const rectangle of rectangles) {
    if (
      !Number.isFinite(rectangle.minX)
      || !Number.isFinite(rectangle.minY)
      || !Number.isFinite(rectangle.maxX)
      || !Number.isFinite(rectangle.maxY)
      || rectangle.minX < 0
      || rectangle.minY < 0
      || rectangle.maxX > viewportWidth
      || rectangle.maxY > viewportHeight
      || rectangle.maxX <= rectangle.minX
      || rectangle.maxY <= rectangle.minY
    ) {
      throw new Error(
        'Screen rectangles must be finite, non-empty, and inside the viewport.',
      )
    }
  }

  if (rectangles.length === 0) {
    return 0
  }

  const xCoordinates = Array.from(
    new Set(
      rectangles.flatMap((rectangle) => [
        rectangle.minX,
        rectangle.maxX,
      ]),
    ),
  ).sort((left, right) => left - right)

  let unionArea = 0

  for (let index = 0; index < xCoordinates.length - 1; index += 1) {
    const stripMinX = xCoordinates[index]
    const stripMaxX = xCoordinates[index + 1]
    const stripWidth = stripMaxX - stripMinX

    const intervals = rectangles
      .filter(
        (rectangle) =>
          rectangle.minX < stripMaxX
          && rectangle.maxX > stripMinX,
      )
      .map((rectangle) => ({
        minY: rectangle.minY,
        maxY: rectangle.maxY,
      }))
      .sort((left, right) => left.minY - right.minY)

    if (intervals.length === 0) {
      continue
    }

    let coveredHeight = 0
    let currentMinY = intervals[0].minY
    let currentMaxY = intervals[0].maxY

    for (const interval of intervals.slice(1)) {
      if (interval.minY <= currentMaxY) {
        currentMaxY = Math.max(currentMaxY, interval.maxY)
        continue
      }

      coveredHeight += currentMaxY - currentMinY
      currentMinY = interval.minY
      currentMaxY = interval.maxY
    }

    coveredHeight += currentMaxY - currentMinY
    unionArea += stripWidth * coveredHeight
  }

  const coverage =
    unionArea / (viewportWidth * viewportHeight)

  return Math.min(1, Math.max(0, coverage))
}

import {
  BoundingSphere,
  Cartesian3,
  type Viewer,
} from 'cesium'

export interface LocalScenePresentationFeature {
  id: string
  measurementPositions: Cartesian3[]
}

export interface LocalSceneViewMeasurement {
  cameraHeightMeters: number
  distanceToLocalSceneMeters: number
  localSceneVisibleFeatureCount: number
  localSceneFeatureBoxCoverage: number
}

export interface LocalSceneViewMeasurementAdapter {
  measure(): LocalSceneViewMeasurement
}

export function createLocalSceneViewMeasurementAdapter(
  viewer: Viewer,
  features: readonly LocalScenePresentationFeature[],
): LocalSceneViewMeasurementAdapter {
  const scenePositions =
    features.flatMap((feature) => feature.measurementPositions)

  if (scenePositions.length === 0) {
    throw new Error(
      'Local-scene view measurement requires at least one position.',
    )
  }

  const sceneBoundingSphere =
    BoundingSphere.fromPoints(scenePositions)

  const cameraToPosition = new Cartesian3()

  return {
    measure(): LocalSceneViewMeasurement {
      const cameraHeightMeters =
        viewer.camera.positionCartographic.height

      const distanceToLocalSceneMeters =
        viewer.camera.distanceToBoundingSphere(
          sceneBoundingSphere,
        )

      const canvasWidth =
        viewer.scene.canvas.clientWidth

      const canvasHeight =
        viewer.scene.canvas.clientHeight

      const viewportArea =
        canvasWidth * canvasHeight

      let localSceneVisibleFeatureCount = 0
      let aggregateVisibleFeatureBoxArea = 0

      for (const feature of features) {
        let featureMinX = Number.POSITIVE_INFINITY
        let featureMinY = Number.POSITIVE_INFINITY
        let featureMaxX = Number.NEGATIVE_INFINITY
        let featureMaxY = Number.NEGATIVE_INFINITY
        let projectedPositionCount = 0

        for (const position of feature.measurementPositions) {
          Cartesian3.subtract(
            position,
            viewer.camera.positionWC,
            cameraToPosition,
          )

          if (
            Cartesian3.dot(
              cameraToPosition,
              viewer.camera.directionWC,
            ) <= 0
          ) {
            continue
          }

          const projected =
            viewer.scene.cartesianToCanvasCoordinates(position)

          if (projected === undefined) {
            continue
          }

          projectedPositionCount += 1
          featureMinX = Math.min(featureMinX, projected.x)
          featureMinY = Math.min(featureMinY, projected.y)
          featureMaxX = Math.max(featureMaxX, projected.x)
          featureMaxY = Math.max(featureMaxY, projected.y)
        }

        if (projectedPositionCount === 0) {
          continue
        }

        const clippedMinX =
          Math.max(0, featureMinX)

        const clippedMinY =
          Math.max(0, featureMinY)

        const clippedMaxX =
          Math.min(canvasWidth, featureMaxX)

        const clippedMaxY =
          Math.min(canvasHeight, featureMaxY)

        const visibleWidth =
          Math.max(0, clippedMaxX - clippedMinX)

        const visibleHeight =
          Math.max(0, clippedMaxY - clippedMinY)

        const visibleArea =
          visibleWidth * visibleHeight

        if (visibleArea <= 0) {
          continue
        }

        localSceneVisibleFeatureCount += 1
        aggregateVisibleFeatureBoxArea += visibleArea
      }

      const localSceneFeatureBoxCoverage =
        viewportArea > 0
          ? aggregateVisibleFeatureBoxArea / viewportArea
          : 0

      return {
        cameraHeightMeters,
        distanceToLocalSceneMeters,
        localSceneVisibleFeatureCount,
        localSceneFeatureBoxCoverage,
      }
    },
  }
}

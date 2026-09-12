export interface PresentationViewContext {
  cameraHeightMeters: number
  localRepresentationScreenSignificance: number
}

export interface PresentationState {
  localStructuresVisible: boolean
}

export interface PresentationPolicy {
  localStructuresEnterHeightMeters: number
  localStructuresExitHeightMeters: number
}

export const defaultPresentationPolicy: PresentationPolicy = {
  localStructuresEnterHeightMeters: 2_000,
  localStructuresExitHeightMeters: 3_000,
}

export function selectPresentationState(
  view: PresentationViewContext,
  previous: PresentationState,
  policy: PresentationPolicy = defaultPresentationPolicy,
): PresentationState {
  if (!Number.isFinite(view.cameraHeightMeters)) {
    throw new Error('cameraHeightMeters must be finite.')
  }

  if (
    !Number.isFinite(view.localRepresentationScreenSignificance)
    || view.localRepresentationScreenSignificance < 0
    || view.localRepresentationScreenSignificance > 1
  ) {
    throw new Error(
      'localRepresentationScreenSignificance must be a finite number from 0 to 1.',
    )
  }

  if (
    !Number.isFinite(policy.localStructuresEnterHeightMeters)
    || !Number.isFinite(policy.localStructuresExitHeightMeters)
    || policy.localStructuresEnterHeightMeters < 0
    || policy.localStructuresExitHeightMeters
      <= policy.localStructuresEnterHeightMeters
  ) {
    throw new Error('Presentation policy thresholds are invalid.')
  }

  const hasScreenSignificance =
    view.localRepresentationScreenSignificance > 0

  if (previous.localStructuresVisible) {
    return {
      localStructuresVisible:
        hasScreenSignificance
        && view.cameraHeightMeters <= policy.localStructuresExitHeightMeters,
    }
  }

  return {
    localStructuresVisible:
      hasScreenSignificance
      && view.cameraHeightMeters < policy.localStructuresEnterHeightMeters,
  }
}

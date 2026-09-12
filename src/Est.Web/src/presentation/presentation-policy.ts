export interface PresentationViewContext {
  cameraHeightMeters: number
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
  if (!Number.isFinite(view.cameraHeightMeters) || view.cameraHeightMeters < 0) {
    throw new Error('cameraHeightMeters must be a finite non-negative number.')
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

  if (previous.localStructuresVisible) {
    return {
      localStructuresVisible:
        view.cameraHeightMeters <= policy.localStructuresExitHeightMeters,
    }
  }

  return {
    localStructuresVisible:
      view.cameraHeightMeters < policy.localStructuresEnterHeightMeters,
  }
}

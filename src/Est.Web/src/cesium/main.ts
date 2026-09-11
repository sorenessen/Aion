import 'cesium/Build/Cesium/Widgets/widgets.css'
import '../style.css'
import { EstApi } from '../api/est-api'

import {
  Cartesian3,
  Color,
  createWorldTerrainAsync,
  ImageryLayer,
  Ion,
  JulianDate,
  Material,
  Rectangle,
  SingleTileImageryProvider,
  TileMapServiceImageryProvider,
  Viewer,
  WebMapServiceImageryProvider,
} from 'cesium'

const token = import.meta.env.VITE_CESIUM_ION_TOKEN

if (!token) {
  throw new Error(
    'VITE_CESIUM_ION_TOKEN is required for the Cesium evaluation.',
  )
}

Ion.defaultAccessToken = token

const app = document.querySelector<HTMLDivElement>('#app')

if (!app) {
  throw new Error('Application root was not found.')
}

app.innerHTML = `
  <div id="cesiumContainer" aria-label="Est Cesium globe evaluation"></div>

  <section class="render-evaluation-panel">
    <div class="render-evaluation-heading">
      <strong>Est Rendering Evaluation</strong>
      <span id="lookLabel">Baseline</span>
    </div>

    <div class="render-evaluation-actions">
      <button id="baselineButton" type="button">Baseline</button>
      <button id="estButton" type="button">Terrain Study</button>
      <button id="landCoverButton" type="button">Land Cover</button>
      <button id="estSurfaceButton" type="button">Est Surface Study</button>
      <button id="estSurfaceTmsButton" type="button">Est Surface TMS</button>
      <button id="visualSurfaceButton" type="button">Est Visual Surface</button>
      <button id="continuousSurfaceButton" type="button">Est Continuous Surface</button>
      <button id="daylightButton" type="button" aria-pressed="false">Lighting: Real Time</button>
    </div>

    <div id="sessionStatus">No simulation session selected.</div>
  </section>
`

const terrainProvider = await createWorldTerrainAsync({
  requestVertexNormals: true,
  requestWaterMask: true,
})

const imageryLayer = ImageryLayer.fromWorldImagery({})

const landCoverProvider = new WebMapServiceImageryProvider({
  url: 'https://dmsdata.cr.usgs.gov/geoserver/mrlc_Land-Cover-Native_conus_year_data/wms',
  layers: 'Land-Cover-Native_conus_year_data',
  parameters: {
    transparent: true,
    format: 'image/png',
  },
  rectangle: Rectangle.fromDegrees(-124.5, 45.5, -120.0, 48.5),
  credit: 'USGS / MRLC Annual NLCD',
})

const viewer = new Viewer('cesiumContainer', {
  terrainProvider,
  baseLayer: imageryLayer,
  animation: false,
  baseLayerPicker: false,
  fullscreenButton: false,
  geocoder: false,
  homeButton: false,
  infoBox: false,
  navigationHelpButton: false,
  sceneModePicker: false,
  selectionIndicator: false,
  timeline: false,
})

viewer.scene.backgroundColor = Color.BLACK
viewer.scene.globe.enableLighting = true
viewer.scene.globe.depthTestAgainstTerrain = true
viewer.scene.globe.showGroundAtmosphere = true
viewer.scene.globe.dynamicAtmosphereLighting = true

viewer.camera.setView({
  destination: Cartesian3.fromDegrees(
    -119.6,
    37.75,
    12_000_000,
  ),
})

function requireElement<T extends HTMLElement>(
  selector: string,
): T {
  const element = document.querySelector<T>(selector)

  if (!element) {
    throw new Error(`Required element was not found: ${selector}`)
  }

  return element
}

const baselineButton =
  requireElement<HTMLButtonElement>('#baselineButton')

const estButton =
  requireElement<HTMLButtonElement>('#estButton')

const landCoverButton =
  requireElement<HTMLButtonElement>('#landCoverButton')

const estSurfaceButton =
  requireElement<HTMLButtonElement>('#estSurfaceButton')

const estSurfaceTmsButton =
  requireElement<HTMLButtonElement>('#estSurfaceTmsButton')

const visualSurfaceButton =
  requireElement<HTMLButtonElement>('#visualSurfaceButton')

const continuousSurfaceButton =
  requireElement<HTMLButtonElement>('#continuousSurfaceButton')

const daylightButton =
  requireElement<HTMLButtonElement>('#daylightButton')

const lookLabel =
  requireElement<HTMLSpanElement>('#lookLabel')

const sessionStatus =
  requireElement<HTMLDivElement>('#sessionStatus')

const sessionId =
  new URLSearchParams(window.location.search).get('session')

if (sessionId) {
  const api = new EstApi('/api')

  try {
    const [session, world] = await Promise.all([
      api.getSession(sessionId),
      api.getWorld(sessionId),
    ])

    const planet = world.planets[0]

    sessionStatus.textContent = planet
      ? `${planet.name} · ${planet.environment.meanSurfaceTemperatureKelvin.toFixed(2)} K · t=${session.currentTimeSeconds}s`
      : `Session ${session.sessionId} · no planets`
  } catch (error) {
    console.error(error)
    sessionStatus.textContent = 'Simulation session could not be loaded.'
  }
}

const ramp = document.createElement('canvas')
ramp.width = 256
ramp.height = 1

const context = ramp.getContext('2d')

if (!context) {
  throw new Error('Could not create terrain color ramp.')
}

const gradient = context.createLinearGradient(0, 0, 256, 0)

gradient.addColorStop(0.00, '#092b46')
gradient.addColorStop(0.18, '#14506a')
gradient.addColorStop(0.28, '#287d8e')
gradient.addColorStop(0.30, '#d1c6a2')
gradient.addColorStop(0.36, '#52734c')
gradient.addColorStop(0.48, '#71835a')
gradient.addColorStop(0.62, '#a79b77')
gradient.addColorStop(0.78, '#817e78')
gradient.addColorStop(0.91, '#c4c5c0')
gradient.addColorStop(1.00, '#f2f3f0')

context.fillStyle = gradient
context.fillRect(0, 0, ramp.width, ramp.height)

const terrainMaterial = Material.fromType(
  Material.ElevationRampType,
  {
    image: ramp,
    minimumHeight: -1000,
    maximumHeight: 6000,
  },
)

const landCoverLayer = viewer.imageryLayers.addImageryProvider(
  landCoverProvider,
)
landCoverLayer.show = false

const surfaceManifestResponse = await fetch(
  '/evaluation/nlcd-2025/surface-manifest.json',
)

if (!surfaceManifestResponse.ok) {
  throw new Error(
    `Est surface manifest could not be loaded: ${surfaceManifestResponse.status}`,
  )
}

const surfaceManifest = await surfaceManifestResponse.json() as {
  geographicPreview: {
    bounds: {
      west: number
      south: number
      east: number
      north: number
    }
  }
}

const surfaceBounds = surfaceManifest.geographicPreview.bounds

const surfaceRectangle = Rectangle.fromDegrees(
  surfaceBounds.west,
  surfaceBounds.south,
  surfaceBounds.east,
  surfaceBounds.north,
)

const surfaceProvider = await SingleTileImageryProvider.fromUrl(
  '/evaluation/nlcd-2025/surface-preview-geographic.png',
  {
    rectangle: surfaceRectangle,
    credit: 'USGS Annual NLCD 2025 / Est Surface Study',
  },
)

const surfaceLayer = viewer.imageryLayers.addImageryProvider(
  surfaceProvider,
)
surfaceLayer.show = false

const surfaceTmsProvider =
  await TileMapServiceImageryProvider.fromUrl(
    '/evaluation/nlcd-2025/surface-tms/',
    {
      credit: 'USGS Annual NLCD 2025 / Est Surface TMS Study',
    },
  )

const surfaceTmsLayer =
  viewer.imageryLayers.addImageryProvider(
    surfaceTmsProvider,
  )

surfaceTmsLayer.show = false

const visualSurfaceTmsProvider =
  await TileMapServiceImageryProvider.fromUrl(
    '/evaluation/nlcd-2025/visual-surface-tms/',
    {
      credit: 'USGS Annual NLCD 2025 / USGS 3DEP / Est Visual Surface Study',
    },
  )

const visualSurfaceTmsLayer =
  viewer.imageryLayers.addImageryProvider(
    visualSurfaceTmsProvider,
  )

visualSurfaceTmsLayer.show = false

const continuousSurfaceTmsProvider =
  await TileMapServiceImageryProvider.fromUrl(
    '/evaluation/nlcd-2025/continuous-surface-tms/',
    {
      credit: 'Copernicus Sentinel-2 / Est Continuous Visual Surface Study',
    },
  )

const continuousSurfaceTmsLayer =
  viewer.imageryLayers.addImageryProvider(
    continuousSurfaceTmsProvider,
  )

continuousSurfaceTmsLayer.show = false

const inspectionDaylightTime =
  JulianDate.fromIso8601('2026-06-21T20:00:00Z')

let inspectionDaylightEnabled = false

function applyInspectionLighting(): void {
  inspectionDaylightEnabled = !inspectionDaylightEnabled

  viewer.clock.currentTime = inspectionDaylightEnabled
    ? inspectionDaylightTime.clone()
    : JulianDate.now()

  daylightButton.textContent = inspectionDaylightEnabled
    ? 'Lighting: Inspection Daylight'
    : 'Lighting: Real Time'

  daylightButton.setAttribute(
    'aria-pressed',
    String(inspectionDaylightEnabled),
  )
}

function applyBaseline(): void {
  viewer.scene.globe.material = undefined
  imageryLayer.show = true
  landCoverLayer.show = false
  surfaceLayer.show = false
  surfaceTmsLayer.show = false
  visualSurfaceTmsLayer.show = false
  continuousSurfaceTmsLayer.show = false

  viewer.scene.globe.lambertDiffuseMultiplier = 1
  viewer.scene.globe.atmosphereLightIntensity = 10

  imageryLayer.brightness = 1
  imageryLayer.contrast = 1
  imageryLayer.saturation = 1
  imageryLayer.gamma = 1

  lookLabel.textContent = 'Baseline'
}

function applyEstLook(): void {
  landCoverLayer.show = false
  surfaceLayer.show = false
  surfaceTmsLayer.show = false
  visualSurfaceTmsLayer.show = false
  continuousSurfaceTmsLayer.show = false
  imageryLayer.show = false
  viewer.scene.globe.material = terrainMaterial

  viewer.scene.globe.lambertDiffuseMultiplier = 1.15
  viewer.scene.globe.atmosphereLightIntensity = 12

  lookLabel.textContent = 'Terrain Study'
}

function applyLandCover(): void {
  viewer.scene.globe.material = undefined
  imageryLayer.show = false
  landCoverLayer.show = true
  surfaceLayer.show = false
  surfaceTmsLayer.show = false
  visualSurfaceTmsLayer.show = false
  continuousSurfaceTmsLayer.show = false

  viewer.scene.globe.lambertDiffuseMultiplier = 1
  viewer.scene.globe.atmosphereLightIntensity = 10

  lookLabel.textContent = 'USGS Land Cover'

}

function applyEstSurface(): void {
  viewer.scene.globe.material = undefined
  imageryLayer.show = false
  landCoverLayer.show = false
  surfaceLayer.show = true
  surfaceTmsLayer.show = false
  visualSurfaceTmsLayer.show = false
  continuousSurfaceTmsLayer.show = false

  viewer.scene.globe.lambertDiffuseMultiplier = 1
  viewer.scene.globe.atmosphereLightIntensity = 10

  lookLabel.textContent = 'Est Surface Study'

}

function applyEstSurfaceTms(): void {
  viewer.scene.globe.material = undefined
  imageryLayer.show = false
  landCoverLayer.show = false
  surfaceLayer.show = false
  surfaceTmsLayer.show = true
  visualSurfaceTmsLayer.show = false
  continuousSurfaceTmsLayer.show = false

  viewer.scene.globe.lambertDiffuseMultiplier = 1
  viewer.scene.globe.atmosphereLightIntensity = 10

  lookLabel.textContent = 'Est Surface TMS'

}

function applyVisualSurface(): void {
  viewer.scene.globe.material = undefined
  imageryLayer.show = false
  landCoverLayer.show = false
  surfaceLayer.show = false
  surfaceTmsLayer.show = false
  visualSurfaceTmsLayer.show = true
  continuousSurfaceTmsLayer.show = false

  viewer.scene.globe.lambertDiffuseMultiplier = 1
  viewer.scene.globe.atmosphereLightIntensity = 10

  lookLabel.textContent = 'Est Visual Surface'
}

function applyContinuousSurface(): void {
  viewer.scene.globe.material = undefined
  imageryLayer.show = false
  landCoverLayer.show = false
  surfaceLayer.show = false
  surfaceTmsLayer.show = false
  visualSurfaceTmsLayer.show = false
  continuousSurfaceTmsLayer.show = true

  viewer.scene.globe.lambertDiffuseMultiplier = 1
  viewer.scene.globe.atmosphereLightIntensity = 10

  lookLabel.textContent = 'Est Continuous Surface'
}

estSurfaceButton.addEventListener('click', applyEstSurface)
estSurfaceTmsButton.addEventListener(
  'click',
  applyEstSurfaceTms,
)
visualSurfaceButton.addEventListener(
  'click',
  applyVisualSurface,
)
continuousSurfaceButton.addEventListener(
  'click',
  applyContinuousSurface,
)
daylightButton.addEventListener(
  'click',
  applyInspectionLighting,
)

baselineButton.addEventListener('click', applyBaseline)
estButton.addEventListener('click', applyEstLook)
landCoverButton.addEventListener('click', applyLandCover)

applyBaseline()

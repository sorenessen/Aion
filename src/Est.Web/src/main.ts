import './style.css'

import {
  ArcRotateCamera,
  Color3,
  Color4,
  DirectionalLight,
  Engine,
  HemisphericLight,
  MeshBuilder,
  PBRMaterial,
  Scene,
  StandardMaterial,
  Vector3,
  VertexBuffer,
} from '@babylonjs/core'

const app = document.querySelector<HTMLDivElement>('#app')

if (!app) {
  throw new Error('Application root was not found.')
}

app.innerHTML = `
  <canvas id="renderCanvas" aria-label="Est planet viewport"></canvas>
`

const canvas = document.querySelector<HTMLCanvasElement>('#renderCanvas')

if (!canvas) {
  throw new Error('Render canvas was not created.')
}

const engine = new Engine(canvas, true, {
  preserveDrawingBuffer: true,
  stencil: true,
})

const scene = new Scene(engine)
scene.clearColor = new Color4(0.002, 0.004, 0.009, 1)

const camera = new ArcRotateCamera(
  'planet-camera',
  Math.PI * 1.25,
  Math.PI * 0.42,
  3.2,
  Vector3.Zero(),
  scene,
)

camera.attachControl(canvas, true)
camera.lowerRadiusLimit = 1.35
camera.upperRadiusLimit = 12
camera.wheelPrecision = 35
camera.panningSensibility = 0
camera.inertia = 0.82

const sunlight = new DirectionalLight(
  'sunlight',
  new Vector3(-0.8, -0.25, 0.5),
  scene,
)

sunlight.intensity = 3.2

const ambient = new HemisphericLight(
  'ambient',
  new Vector3(0, 1, 0),
  scene,
)

ambient.intensity = 0.12

const planet = MeshBuilder.CreateSphere(
  'planet',
  {
    diameter: 2,
    segments: 192,
    updatable: true,
  },
  scene,
)

const positions = planet.getVerticesData(
  VertexBuffer.PositionKind,
)

const colors: number[] = []

if (!positions) {
  throw new Error('Planet geometry was not created.')
}

function noise(x: number, y: number, z: number): number {
  return (
    Math.sin(x * 3.7 + Math.cos(z * 2.1)) *
    Math.cos(y * 4.3 - Math.sin(x * 1.7)) *
    0.5 +
    Math.sin(x * 9.1 + y * 5.3 + z * 3.7) * 0.18 +
    Math.cos(x * 17.3 - y * 11.7 + z * 7.9) * 0.07
  )
}

const ocean = new Color3(0.012, 0.095, 0.17)
const shallow = new Color3(0.025, 0.22, 0.27)
const lowland = new Color3(0.12, 0.24, 0.13)
const highland = new Color3(0.31, 0.29, 0.19)
const mountain = new Color3(0.52, 0.49, 0.42)
const ice = new Color3(0.76, 0.86, 0.91)

function blend(a: Color3, b: Color3, t: number): Color3 {
  return Color3.Lerp(a, b, Math.max(0, Math.min(1, t)))
}

for (let i = 0; i < positions.length; i += 3) {
  const x = positions[i]
  const y = positions[i + 1]
  const z = positions[i + 2]

  const elevation = noise(x, y, z)
  const latitude = Math.abs(y)

  let color: Color3

  if (elevation < -0.08) {
    color = blend(
      ocean,
      shallow,
      (elevation + 0.6) / 0.52,
    )
  } else if (elevation < 0.02) {
    color = blend(
      shallow,
      lowland,
      (elevation + 0.08) / 0.1,
    )
  } else if (elevation < 0.28) {
    color = blend(
      lowland,
      highland,
      (elevation - 0.02) / 0.26,
    )
  } else {
    color = blend(
      highland,
      mountain,
      (elevation - 0.28) / 0.3,
    )
  }

  const polarIce = Math.max(
    0,
    Math.min(1, (latitude - 0.72) / 0.16),
  )

  color = blend(color, ice, polarIce)

  colors.push(color.r, color.g, color.b, 1)
}

planet.setVerticesData(
  VertexBuffer.ColorKind,
  colors,
)

const surface = new PBRMaterial(
  'planet-surface',
  scene,
)

surface.albedoColor = Color3.White()
surface.metallic = 0
surface.roughness = 0.92
surface.environmentIntensity = 0
planet.useVertexColors = true

planet.material = surface

const atmosphere = MeshBuilder.CreateSphere(
  'atmosphere',
  {
    diameter: 2.08,
    segments: 96,
  },
  scene,
)

const atmosphereMaterial = new StandardMaterial(
  'atmosphere-material',
  scene,
)

atmosphereMaterial.diffuseColor = new Color3(
  0.08,
  0.28,
  0.55,
)

atmosphereMaterial.emissiveColor = new Color3(
  0.015,
  0.055,
  0.12,
)

atmosphereMaterial.alpha = 0.055
atmosphereMaterial.backFaceCulling = false
atmosphereMaterial.disableLighting = true
atmosphereMaterial.disableDepthWrite = true

atmosphere.material = atmosphereMaterial

engine.runRenderLoop(() => {
  scene.render()
})

window.addEventListener('resize', () => {
  engine.resize()
})

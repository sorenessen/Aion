import definition from './surface-categories.json'

type SurfaceCategoryDefinition =
  (typeof definition.categories)[keyof typeof definition.categories]

export type SurfaceCategory = SurfaceCategoryDefinition['value']

export const SurfaceCategory = Object.fromEntries(
  Object.entries(definition.categories).map(
    ([name, category]) => [name, category.value],
  ),
) as {
  [Name in keyof typeof definition.categories]:
    (typeof definition.categories)[Name]['value']
}

export const SurfaceCategoryDefinition = definition

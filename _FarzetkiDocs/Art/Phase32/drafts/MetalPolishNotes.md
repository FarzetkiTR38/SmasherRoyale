# Phase32 metal polish candidate — V2

The shader and helper are staged outside Assets for root integration and Unity verification. V2 retains the reviewed DepthNormals pass and adds per-face UV0 polish. `_FaceContrast` blends local smooth dark-to-bright face shading and a broad smooth highlight band with the existing object wave; `_HighlightBlend` allows brighter colored gold/pale blue strips. Raw face UV is carried separately from transformed albedo UV in the existing TEXCOORD0, so texture tiling does not change polish size.

Gold V2: ShadeStrength1, DarkTint(.18,.095,.025), FaceContrast.82, HighlightBlend.58, PolishSpecular.55. Sapphire V2: ShadeStrength.9, DarkTint(.10,.17,.35), HighlightTint(.60,.83,1), FaceContrast.90, HighlightBlend.78, PolishSpecular.40. Original base colors, metallic/smoothness, maps and render state remain preserved.

Local UV.y is saturated before smoothsteps. This bounds lathe UVs outside [0,1] without wrapping; those regions can have uniform local tone. Fillet orientation/UV seams and curved-ring appearance require root A/B inspection. No screen-space texture or emissive polish is introduced.

The current material colors, metallic and smoothness values are retained. The two source materials are opaque URP Lit, have no assigned detail/normal/metallic/occlusion maps, and have no active keywords. The helper also supports preserving BaseMap, EmissionMap, alpha clipping, smoothness from albedo alpha, receive-shadow, specular-highlight and environment-reflection keywords. Unsupported assigned maps, transparent/specular workflow, clear coat, unsaved material edits and unsupported active keywords are rejected before mutation.

## Appearance

- A smooth object-position wave creates broad warm-dark body variation. Coordinates use the object-to-world linear transform without translation, so the finish is anchored to each mesh and has a metre-based scale; it cannot follow the camera or crawl when the object moves.
- A small projected-axis normal bend adds broad specular/reflection variation across planar caps. Existing geometry and shadow/depth positions are unchanged.
- A broad analytical specular strip is tinted by the original material and a restrained highlight color. It uses the actual main light, its shadow/distance attenuation, AO and light layer. Ordinary URP PBR still handles all scene lights and the existing probe. No additional reflection render or lookup, extra pass, texture asset, collider or geometry is introduced.
- A small curvature accent comes from derivatives of existing rounded mesh normals. It is multiplied by the same directional lobe, light-facing term and shadow attenuation; it is not a global outline.
- Polish does not enter the emission channel. Existing legitimate emission remains supported; these two source materials have black emission.

## Analytical check

`32-12-metal-polish-v2-math.json` samples 256 object phases × 256 local face heights on a front-facing planar surface using the saved main-light quaternion/color/intensity and the actual gold/sapphire base colors converted from sRGB to linear. It records the stronger V2 deliberate body/strip terms only, not the full rendered PBR/probe/bloom/tonemapped result. The V1 report is retained as historical comparison.

The deliberate strip remains colored gold rather than an all-white additive band. Actual brightest URP specular pixels still require Unity A/B inspection.

Sapphire uses a lighter blue highlight color and stronger local body contrast to address the weak V1 variation. It remains lit and shadow-attenuated rather than becoming an emissive panel.

## Integration and rollback

Copy the shader into the existing SkyCitadel Shaders folder and the helper into the Editor scripts folder. After successful compilation, `Phase32MetalPolishRevision.Apply()` updates only the shader and custom polish properties of the existing gold/sapphire material assets. It preserves material GUID/meta, base/emission colors, maps/tiling, metallic/smoothness, render state and supported active keywords. Reapplying an unchanged preset is a no-op.

Apply returns a rollback directory under the current user profile's `.codex/local-backups/Phase32MetalPolish/`. Backups stay outside the repository. `Rollback(directory)` verifies the fixed two-material manifest, original SHA256 bytes, metadata and GUIDs before restoring. Failure recovery attempts every material's in-memory restore and every file write/import, collecting errors rather than abandoning the second material.

The installed URP package source was checked for `GetMainLight(InputData, shadowMask, AO)`, `UniversalFragmentPBR`, `TransformObjectToWorldDir`, alpha helper dependencies, and shadow/depth pass APIs. A single identical UnityPerMaterial CBUFFER is supplied through HLSLINCLUDE. This subagent did not import or compile the candidate in Unity; root must verify compilation, fixed-camera A/B appearance, shadow/depth behavior, both screen ratios and render cost before keeping it.

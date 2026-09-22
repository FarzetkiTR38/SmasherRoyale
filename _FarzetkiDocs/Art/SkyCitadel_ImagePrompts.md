# Sky Citadel — Image prompts

## T_CitadelMarble.png

- Date: 2026-09-20
- Method: built-in `image_gen` tool; one generation, no CLI fallback.
- Reference inspected: `C:/Users/Farzetki/Downloads/Codex Görseli 20 Eyl 2026 19_18_21.png` (visual style reference only; no image input supplied to generation).
- Source: `C:/Users/Farzetki/.codex/generated_images/01a0bfa9-065c-75a1-ac0b-d96698ba248d/exec-0ba1415a-cfb6-412a-b475-18ab91dfe8cf.png`
- Project asset: `Assets/_Project/Art/SkyCitadel/Textures/T_CitadelMarble.png`
- Inspection: square orthographic ivory marble paving, narrow muted gold joints, sparse slate-blue diamond inlays. No scenery, text, UI, or cast shadows. Opposing geometric tile borders visually align; seamless appearance should also be reviewed with Unity Repeat wrapping at final floor scale.

### Exact final prompt

```text
Use case: stylized-concept
Asset type: ONE square seamless tileable PBR base-color texture for a Unity mobile fantasy game courtyard floor.
Primary request: a flat orthographic top-down texture map of luxurious warm ivory marble paving, thin muted gold grout/inlay strips, and occasional small pale slate-blue diamond-shaped stone inlays where four tiles meet. Repeatable elegant 2 by 2 square marble paving pattern, large cream slabs with clipped corners, subtle natural beige and cream marble veining. Small blue diamonds should occupy less than 5 percent of the image; gold lines should be narrow. Match a bright ivory-and-gold floating castle courtyard with blue roofs, while keeping this asset ONLY a floor material map.
Style/medium: polished stylized game material texture; softly hand-painted natural stone details, refined and restrained, production-ready.
Composition/framing: 1024 by 1024 square image filling the full canvas. Perfectly top-down orthographic, zero perspective. Repeating grid lines parallel to image edges. Opposite image borders must match perfectly for seamless repetition in both axes; no border/frame.
Lighting: completely uniform neutral diffuse albedo lighting, no directional light, no ambient occlusion, no baked highlights, no cast shadows, no gradients or vignette.
Color palette: warm ivory cream, delicate beige marble veins, narrow muted golden lines, sparse desaturated light slate-blue diamonds.
Constraints: opaque full-canvas material. NO sky, horizon, castle, architecture beyond the paving, game objects, decoration above floor, scenery, labels, letters, text, UI, watermark, perspective, bevel shadows, raised surfaces, ornate medallion, central focal point. Output exactly one texture map.
```


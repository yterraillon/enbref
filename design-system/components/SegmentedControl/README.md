# SegmentedControl

Choix exclusif entre deux à quatre options courtes, affichées en même temps (Apparence : Clair / Sombre / Auto).

- **Fournir** : `options` (libellés d'un mot), `value` + `onChange` ou `defaultValue`, `label` pour l'accessibilité.
- Piste `fill-tertiary` rayon `radius-md`, segment actif `segment-selected` + `shadow-segment` rayon `radius-sm`, texte style `control`.
- Dans une section groupée, l'envelopper avec un padding 10 × 12 px.
- Au-delà de quatre options, passer à `ChoiceChips`.

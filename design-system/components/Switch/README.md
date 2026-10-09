# Switch

Interrupteur on/off au format iOS (51 × 31 px), pour un réglage qui s'applique immédiatement.

- **Fournir** : `checked` + `onChange` (contrôlé) ou `defaultChecked`, et `label` si aucun libellé visible n'est à côté.
- Piste `switch-on` (vert système) quand activé, `switch-off` sinon ; bouton `switch-knob` avec `shadow-knob`. Transition 0,2 s.
- Placer dans un `ListRow` via `trailing`. Sur iOS, utiliser le `Toggle` SwiftUI natif avec `.tint(.green)` par défaut.
- Ne pas utiliser pour une action à confirmer ou qui navigue.

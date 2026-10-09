# MiniPlayer

Mini-lecteur flottant affiché pendant l'écoute, collé en bas de l'écran.

- **Fournir** : `title` (rubrique en cours), `rate` (« 1× », « 1,25× », « 1,5× »), `playing`, `onRate`, `onNext`, `onToggle`.
- Matière `material-player` floutée 24 px, rayon `radius-player`, ombre `shadow-player`, à `space-12` des bords et 28 px du bas.
- Surtitre « EnBref · en lecture » en `caption` `label-secondary`, titre en `callout`.
- N'apparaît que pendant la lecture ; sur iOS, doubler par les contrôles de l'écran verrouillé (`MPNowPlayingInfoCenter`).

# web

Site vitrine d'EnBref : landing, À propos, CGU, confidentialité, mentions légales, politique de
suivi, support. **HTML/CSS statique, sans build ni dépendances.**

En ligne : <https://yterraillon.github.io/pages/enbref/>

## Le choix : pas de framework, pas de toolchain

Le site présente l'application et renvoie vers l'App Store. Il ne consomme pas le récap, n'a aucune
partie dynamique, et change quelques fois par an.

Dans ces conditions un générateur de site statique coûterait plus qu'il ne rapporterait : une
`node_modules` à maintenir, des mises à jour Dependabot à trier, une étape de build entre la
modification et la mise en ligne. On écrit du HTML et du CSS.

Corollaire assumé : pas de templates ni de partials. Si le site grossit au point que la duplication
devient pénible, c'est le moment de rouvrir la décision — par un ADR, pas en ajoutant un `package.json`.

## Contenu

| Fichier | Rôle |
|---|---|
| `index.html` | landing |
| `about-us.html`, `privacy-policy.html`, `terms-of-use.html`, `legal-notice.html`, `app-tracking-policy.html` | pages de texte |
| `support.html` | formulaire Tally + adresse de support |
| `styles.css` | mise en page du site, uniquement en variables du design system |
| `images/` | favicons (icône d'app) et capture d'écran |
| `.htmlvalidate.json` | règles du lint HTML (CI) |

Le design system (`tokens.css`, `bundle.css`) n'est pas commité ici : il est copié dans
`design-system/` au déploiement.

## Publication

`.github/workflows/web.yml` (ADR-010, détail dans [`docs/deployment.md`](../docs/deployment.md)) :

- **PR** : assemblage du site, HTML valide, liens internes, pas de chemin absolu, pas de couleur en dur ;
- **merge sur `main`** : copie dans `pages/enbref/` de `yterraillon/yterraillon.github.io`.

Tous les chemins sont relatifs : le site vit sous `/pages/enbref/`.

## Développement

```bash
mkdir -p web/design-system
cp design-system/tokens.css design-system/components/bundle.css web/design-system/
python -m http.server 8000 --directory web
```

Puis <http://localhost:8000/>.

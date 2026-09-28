# web

Site vitrine d'EnBref. **HTML/CSS statique, sans build ni dépendances.**

> 🚧 **Non porté.** Le site n'est pas encore dans ce dépôt.

## Le choix : pas de framework, pas de toolchain

Le site présente l'application et renvoie vers l'App Store. Il ne consomme pas le récap, n'a aucune
partie dynamique, et change quelques fois par an.

Dans ces conditions un générateur de site statique coûterait plus qu'il ne rapporterait : une
`node_modules` à maintenir, des mises à jour Dependabot à trier, une étape de build entre la
modification et la mise en ligne. On écrit du HTML et du CSS, on dépose les fichiers.

Corollaire assumé : pas de templates ni de partials. Si le site grossit au point que la duplication
devient pénible, c'est le moment de rouvrir la décision — par un ADR, pas en ajoutant un `package.json`.

## Hébergement

Sur l'espace de 100 Mo fourni avec le nom de domaine OVH. Le déploiement consiste à déposer les
fichiers.

## Développement

Ouvrir `index.html` dans un navigateur suffit. Pour un serveur local :

```bash
python -m http.server 8000 --directory web
```

## À faire au portage

- Contenu et maquette.
- Renseigner le nom de domaine dans [`docs/deployment.md`](../docs/deployment.md).
- Décider si le dépôt de fichiers est manuel ou outillé par un workflow.

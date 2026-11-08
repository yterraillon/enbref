# CLAUDE.md — web

Site vitrine d'EnBref : landing, À propos, pages légales, support. **HTML/CSS statique, sans build,
sans dépendances.** Publié sur <https://yterraillon.github.io/pages/enbref/> (ADR-010).

## La contrainte principale

**Pas de toolchain.** Pas de `package.json`, pas de `node_modules`, pas d'étape de build entre la
modification d'un fichier et sa mise en ligne. On écrit du HTML et du CSS ; la CI se contente de
copier le design system à côté et de déposer les fichiers.

Ce choix est délibéré : le site présente l'application, renvoie vers l'App Store, et change quelques
fois par an. Un générateur de site statique y ajouterait des mises à jour de dépendances à trier et
un build à faire tourner, pour un bénéfice nul à cette échelle.

Conséquences assumées :

- pas de templates ni de partials — la duplication entre pages (en-tête, pied, `<head>`) est
  acceptée : **un changement de nav ou de pied se reporte sur les 7 pages** ;
- pas de préprocesseur CSS ; les variables CSS natives suffisent ;
- pas de framework JS. Le seul script du site est le suivi du thème clair/sombre, inline dans le
  `<head>` (plus l'embed Tally sur `support.html`).

Les outils de lint (html-validate, lychee) tournent **dans la CI seulement**, lancés par `npx` ou
une action : rien n'est installé dans le dépôt.

Si le site grossit au point que la duplication devient réellement coûteuse, **rouvrir la décision
par un ADR** — pas en ajoutant discrètement un `package.json`.

## Règles qui cassent la prod si on les oublie

- **Chemins relatifs uniquement** (`./images/…`, `./support.html`). Le site est servi sous
  `/pages/enbref/` : un `/images/…` pointerait à la racine de yterraillon.github.io. La CI le refuse.
- **Design system** : chaque page charge `./design-system/tokens.css`, `./design-system/bundle.css`
  puis `./styles.css`. `web/design-system/` est **ignoré par git** et rempli par la CI ; ne pas le
  commiter. Dans `styles.css`, uniquement des `var(--…)` — la CI refuse toute couleur en dur. Une
  valeur manquante passe d'abord par `design-system/tokens.json`.
- Classes de typo et de composants du design system (`.large-title`, `.headline`, `.eb-badge`…) :
  lire `design-system/README.md` avant toute retouche visuelle.

## Ce que le site ne fait pas

- **Il n'affiche pas le récap.** C'est le rôle de l'application. Le site n'a aucune raison
  d'appeler le CDN.
- **Il ne collecte rien lui-même.** Pas de compte, pas de cookie, pas de mesure d'audience. Seule
  exception : le formulaire Tally (tiers) de `support.html`, mentionné dans la politique de
  confidentialité.

## Développement

```bash
mkdir -p web/design-system
cp design-system/tokens.css design-system/components/bundle.css web/design-system/
python -m http.server 8000 --directory web
```

Recopier les deux feuilles après tout changement du design system.

## Vocabulaire et langue

Site en français, vouvoiement, typographie française (’ « » espaces insécables). Le glossaire
s'applique aux textes affichés : on écrit **récap**, jamais « résumé », « digest » ou « briefing »,
et **catégorie**, pas « rubrique ». Voir `docs/ubiquitous-language.md` § 8. La marque s'écrit
**EnBref**.

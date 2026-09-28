# CLAUDE.md — web

Site vitrine d'EnBref. **HTML/CSS statique, sans build, sans dépendances.**

> 🚧 **Le site n'est pas encore porté.**

## La contrainte principale

**Pas de toolchain.** Pas de `package.json`, pas de `node_modules`, pas d'étape de build entre la
modification d'un fichier et sa mise en ligne. On écrit du HTML et du CSS, on dépose les fichiers
sur l'espace OVH.

Ce choix est délibéré : le site présente l'application, renvoie vers l'App Store, et change quelques
fois par an. Un générateur de site statique y ajouterait des mises à jour de dépendances à trier et
un build à faire tourner, pour un bénéfice nul à cette échelle.

Conséquences assumées :

- pas de templates ni de partials — la duplication entre pages est acceptée ;
- pas de préprocesseur CSS ; les variables CSS natives suffisent ;
- pas de framework JS. Si une interaction demande du script, c'est du JavaScript inline ou un
  fichier `.js` chargé directement, sans bundler.

Si le site grossit au point que la duplication devient réellement coûteuse, **rouvrir la décision
par un ADR** — pas en ajoutant discrètement un `package.json`.

## Ce que le site ne fait pas

- **Il n'affiche pas le récap.** C'est le rôle de l'application. Le site n'a aucune raison
  d'appeler le CDN.
- **Il ne collecte rien.** Pas de formulaire, pas de compte — il n'y a pas d'utilisateur dans
  EnBref.

## Hébergement

Espace de 100 Mo fourni avec le nom de domaine OVH. Le déploiement consiste à déposer les fichiers ;
son outillage éventuel reste à décider.

La taille de l'espace est une contrainte réelle : attention au poids des images et des polices.
Préférer des polices système à des fichiers embarqués.

## Développement

Ouvrir `index.html` suffit. Pour un serveur local :

```bash
python -m http.server 8000 --directory web
```

## Vocabulaire et langue

Site en français. Le glossaire s'applique aux textes affichés : on écrit **récap**, jamais
« résumé », « digest » ou « briefing ». Voir `docs/ubiquitous-language.md` § 8.

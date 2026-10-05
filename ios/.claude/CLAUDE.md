# CLAUDE.md — ios

Application Swift 6 / SwiftUI, **iOS 26 minimum**.

> 🚧 **L'application n'est pas encore portée.** Ce fichier fixe ce qui est déjà décidé. Les
> conventions Swift (structure des vues, gestion d'état, injection) seront écrites au portage, à
> partir du code réel — pas inventées ici.

## Ce que l'app consomme

Un fichier statique sur GitHub Pages :

```
https://yterraillon.github.io/cdn/en-bref/data/latest-recap.json
```

**L'app ne parle jamais au serveur.** Il n'est pas exposé sur internet (ADR-001). Aucune
fonctionnalité ne peut supposer un appel vers l'API : pas d'historique consultable, pas de
personnalisation, pas de contenu par utilisateur. Il n'y a d'ailleurs **ni compte ni utilisateur**
dans EnBref.

Une fonctionnalité qui exigerait un aller-retour serveur remet en cause ADR-001 et demande un
nouvel ADR — ce n'est pas une décision de story.

## Le contrat, et le piège qui va avec

La forme cible du récap est au § 7 du glossaire : sept catégories ordonnées, une à deux brèves
chacune, chaque brève portant un titre et un résumé d'une phrase.

⚠️ **Le serveur ne publie pas encore cette forme.** Il publie aujourd'hui un `Recap` avec un `Title`
et des `Sections { Title, Text }` en texte libre. Partir du contrat **réel**, constaté sur
l'artefact publié, pas de la cible.

⚠️ **Un champ optionnel avale silencieusement une clé renommée.** La propriété devient `nil`, rien
ne lève, et l'écran se vide. Le fait que ça compile ne prouve rien. Deux conséquences :

- décoder de façon stricte là où un champ est réellement obligatoire, plutôt que de tout rendre
  optionnel « pour que ça passe » ;
- à tout changement du contrat serveur, vérifier l'app — ne jamais conclure au non-risque depuis le
  serveur.

## Vocabulaire

Le glossaire s'applique aussi aux libellés affichés et aux noms de types Swift : `Recap`, `Brief`,
`Category`, et les sept catégories dans leur ordre fixe (Politique, International, Économie,
Société, Technologies & Science, Sport, Culture).

`Brief.Title` est le titre **affiché**. `Headline` désigne le titre brut RSS, qui ne parvient jamais
à l'app — ce type n'a donc aucune raison d'exister côté Swift.

## Cache et rafraîchissement — à concevoir

Rien n'est arrêté, mais trois contraintes le sont :

- **GitHub Pages sert avec un cache agressif.** Une revalidation naïve renverra longtemps le même
  contenu. S'appuyer sur `ETag` / `If-Modified-Since`.
- **L'app doit rester lisible hors ligne** : le cas d'usage est le matin dans les transports. Le
  dernier récap connu s'affiche même sans réseau.
- **Le pull to refresh doit dire quelque chose quand rien n'a changé.** « Rien de neuf » est une
  réponse, pas un échec.

## Récap de démo

`demo.json` suit le même contrat et sert à la revue App Store, qui exige un contenu stable. Le
mécanisme de bascule reste à définir.

## Distribution

TestFlight, pipeline outillé par fastlane. Le `.gitignore` de la racine couvre déjà
`ios/fastlane/` (rapports, captures, clés).

Aucune clé de signature, aucun profil, aucun secret App Store Connect dans le dépôt.

# ios

Application Swift 6 / SwiftUI qui affiche le récap du jour. **iOS 26 minimum.**

> 🚧 **Non porté.** Le code de l'application n'est pas encore dans ce dépôt. Ce README décrit la
> cible ; il sera complété au portage.

## Ce que fait l'app

Elle charge le récap du jour au lancement, le conserve localement, et offre un *pull to refresh*
pour aller chercher un récap plus récent.

Elle lit un fichier statique publié sur GitHub Pages :

```
https://yterraillon.github.io/cdn/en-bref/data/latest-recap.json
```

**Elle ne parle jamais au serveur** — celui-ci n'est pas exposé sur internet
([ADR-001](../docs/architecture-decision-record.md)). Conséquence directe : le récap reste
consultable même quand le NAS est éteint, et aucune fonctionnalité de l'app ne peut supposer un
appel vers l'API.

## Le contrat consommé

Le glossaire fixe la forme du récap : sept catégories ordonnées, une à deux brèves par catégorie,
chaque brève portant un titre et un résumé d'une phrase. Voir
[§ 7 du glossaire](../docs/ubiquitous-language.md#7-structure-et-budget-de-lecture).

⚠️ **Le contrat réel diffère encore de la cible** : le serveur publie aujourd'hui un `Recap` avec un
`Title` et des `Sections { Title, Text }` en texte libre. Le portage doit partir du contrat réel,
pas de la cible.

⚠️ **Un champ optionnel en Swift avale silencieusement une clé renommée** : la propriété devient
`nil`, rien ne lève, et l'écran se vide. C'est déjà arrivé sur d'autres projets. Tout changement du
contrat publié doit être vérifié côté app, jamais supposé sans risque parce que ça compile.

## À décider au portage

- **Politique de cache** : durée de validité du récap local, et comportement du *pull to refresh*
  quand rien n'a changé. GitHub Pages sert avec un cache agressif — la revalidation doit s'appuyer
  sur `ETag` / `If-Modified-Since`, sinon le refresh renverra longtemps le même contenu.
- **Hors ligne** : l'app doit rester lisible sans réseau, c'est le cas d'usage du matin dans les
  transports.
- **Récap de démo** : `demo.json` suit le même contrat et sert à la revue App Store. Comment l'app
  y bascule reste à définir.

## Distribution

TestFlight, pipeline outillé par fastlane. Le `.gitignore` de la racine prévoit déjà
`ios/fastlane/`.

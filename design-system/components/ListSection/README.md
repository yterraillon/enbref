# ListSection

Section groupée iOS : en-tête en capitales, carte blanche à 22 px, pied explicatif.

- **Fournir** : `header` (un ou deux mots : « Réception », « Apparence »), `children` (des `ListRow`), `footer` optionnel (une phrase qui explique la conséquence du réglage).
- Carte `surface`, rayon `radius-card`, marge latérale `space-16` ; en-tête et pied en `footnote` `label-secondary`, retrait `space-36`.
- Deux sections consécutives sont séparées par `space-26`.
- Pas d'ombre, pas de bordure : la carte se détache seulement par la couleur sur `bg-grouped`.
- Admin web : même construction pour les pages de réglages et les fiches ; largeur de contenu 640 px max.

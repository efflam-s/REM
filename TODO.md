## Liste de trucs à faire : Projet Wiring

#### A faire maintenant :
* Paramètres de sauvegarde et d'ouverture de schematics
* Sauvegarde des schematics : save les blackbox dans un autre fichier
* Bug : entrée-sorties (fixé ?)
* Warning : Inapropriate number of wires pour les blackbox ?
* Relire et nettoyer le projet entier
* Afficher des infos sur le composant hovered ?
* BlackBox : trier des input/output par hauteur
* BlackBox : agrandir quand il y a trop de fils
* Améliorer la visibilité des prises : afficher un bout de fil pour chaque prise de composant

#### A faire plus tard :
* Petit bug : les fils ne sont pas toujours update dans les blackbox (mais pas de problème global détécté pour l'instant)
* Duplication d'une séléction multiple
* Raccourcis pour les créations de composants (chiffres ?)
* Composant : `Comment`
* **LA DOOOOC !!**
* Curseur : changer les ciseaux
* Ajustement auto de la Camera
* Changer le système de delay : séparer diode/delay ? délais micro (tick) et macro (seconde) ?

#### Eventuellement, si j'ai le courage :
* Panoramique à fix ?
* Angle des composants
* Menu de simulation (pas d'édition possible, possibilité de cacher des composants/fils, performances accrues ?)
* Faire des fils personnalisés : nodes où on veut
* OU : Fils à angle droit
* Créer une blackbox à partir du schematic courant / de la selection
* Fixer le stackoverflow
* Couper/Copier/Coller
* Effets sonores + animations
* Composant `Random` : à chaque impulsion, renvoie  aléatoirement 1 ou 0 jusqu'à la fin de l'impulsion
* Historique des modifications => Annuler, Refaire...
* Menu clic droit
* Selection au lasso
* "Librarie" de blackboxes utiles (and, add, mux...)
* Pouvoir transformer les blackbox en bitmap pour gagner en efficacité
* Ou alors : Composant `Bitmap`

---

## Paramètres...

#### Ouverture

[ ] Ignorer les avertissements<br/>
/!\ Améliore les performances de chargement, déconseillé si vous éditez le code de vos schematics

[ ] Ouvrir dans une nouvelle boîte noire

#### Enregistrement

[ ] Enregistrer les boîtes noires dans des fichiers séparés <br/>
/!\ les boîtes noires de même nom doivent être similaires

[ ] Optimiser la taille du fichier (lisibilité réduite du code)

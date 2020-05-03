## Liste de trucs à faire : Projet Wiring


#### Relecture et Nettoyage du projet :
* Editor
* Schematic
* InputManager
* SchemReader
* SchemWriter
* Settings
* ButtonsBar
* Button
* *les différents boutons*

#### Documentation :
* Les outils : S, H/Z
* Les fils (C)
* Les différents composants

#### A faire maintenant :
* Bug : SchemPath passe devant les tooltips des boutons
* Système de delay : vérifier les bugs / rendre plus propre ?
* Panoramique à fix ?
* Sauvegarde des schematics : save les blackbox dans un autre fichier
* Bouts de fil aux plugs : gérer dans les composants ou dans les fils ?

#### A faire plus tard :
* Composant : `Comment`
* Texture de fond de l'éditeur
* Duplication d'une séléction multiple
* BlackBox : agrandir quand il y a trop de fils
* **LA DOOOOC !!** ![Red](Wiring/Content/WireNodeOn.png)
* Afficher des infos sur le composant hovered ?
* Outil déplacement : flèches directionelles
* Créer une blackbox à partir de la selection ?
* Raccourcis pour les créations de composants (chiffres ?)
* Curseur : changer les ciseaux ![Red](Wiring/Content/WireNodeOn.png)
* Ajustement auto de la Camera

#### Eventuellement, si j'ai le courage :
* Personnalisation des BlackBox : image
* Fichier de settings (settings d'enregistrement + couleurs de texte, tooltip...)
* Angle des composants
* Menu de simulation (pas d'édition possible, possibilité de cacher des composants/fils, performances accrues ?)
* Faire des fils personnalisés : nodes où on veut
* OU : Fils à angle droit
* Fixer le stackoverflow
* Couper/Copier/Coller
* Effets sonores + animations
* Composant `Random` : à chaque impulsion, renvoie  aléatoirement 1 ou 0 jusqu'à la fin de l'impulsion
* Historique des modifications => Annuler, Refaire...
* Menu clic droit
* Selection au lasso
* "Librarie standard" de blackboxes utiles (and, add, mux...) ![Red](Wiring/Content/WireNodeOn.png)
* Pouvoir transformer les blackbox en bitmap pour gagner en efficacité
* Ou alors : Composant `Bitmap`
* Fichier de langue => traduction ![Red](Wiring/Content/WireNodeOn.png)

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

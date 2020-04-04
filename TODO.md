## Liste de trucs à faire : Projet Wiring

#### A faire maintenant :
* Pouvoir rajouter ou enlever des entrée/sorties dans les `BlackBox` *(implémenté mais peut-être à fix)*
* Edition des fils : bug d'update de la création sans changement ? *(je crois que c'est bon)*
* Ajout de composant : non-création si souris hors de l'éditeur et annulation si reclic sur le bouton
* Panoramique à fix ?
* Pas d'update pendant le renommage pose problème ?
* Petit bug : pas de clear de selection pdt la navigation entre les schematics (retour en arrière)
* Curseur : changer les ciseaux

#### A faire plus tard :
* Sauvegarder des schematics
* BlackBox : trier des input/output par hauteur
* Blackbox : afficher le nom dans un tooltip
* Petit bug : les fils ne sont pas toujours update dans les blackbox (mais pas de problème global détécté pour l'instant)
* Angle des composants
* Mettre la couleur du texte dans une texture
* Duplication d'une séléction multiple
* Raccourcis pour les créations de composants (chiffres ?)
* Activer les élements depuis l'outil wire
* Composant : `Comment`
* **LA DOOOOC !!**

#### Eventuellement, si j'ai le courage :
* Menu de simulation (pas d'édition possible, possibilité de cacher des composants/fils, performances accrues ?)
* Faire des fils personnalisés : nodes où on veut
* Fixer le stackoverflow
* Menu clic droit
* Couper/Copier/Coller
* Effets sonores + animations
* Composant `Random` : à chaque impulsion, renvoie  aléatoirement 1 ou 0 jusqu'à la fin de l'impulsion
* Historique des modifications => Annuler, Refaire...
* Ajustement auto de la Camera
* Selection au lasso
* "Librarie" de blackboxes utiles (and, add, mux...)
* Pouvoir transformer les blackbox en bitmap pour gagner en efficacité
* Ou alors : Composant `Bitmap`
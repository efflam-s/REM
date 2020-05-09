## Liste de trucs à faire : Projet Wiring


#### Relecture et Nettoyage du projet :
* Editor

#### Documentation :
* Les outils : S, H/Z
* Les fils (C)
* Les différents composants

#### A faire maintenant :
* Changer le nom du projet
* Mettre sur github et supprimer le repo gitlab

#### A faire plus tard :
* Outil Wire : clic droit pour annuler l'action en cours
* Fichier de settings (settings d'enregistrement + couleurs de texte, tooltip...)
* Composant : `Comment`
* Bug : annuler un enregistrement sauvegarde quand même un chemin
* Bug : affichage du fil allumé pdt la création pour une blackbox sans plugs
* Texture de fond de l'éditeur
* ? Faire en sorte que échap ne demande pas la création d'une nouvelle blackbox
* Améliorer l'UI : mettre un hover (+ toggle ?) dans UIObject
* ? Séparer les fils d'input et d'output dans les composants
* ? Revoir de système de tools : boutons pour chaque, boolean toogle, tool Wire à partir d'un plug
* Cursor création de fil
* Couper/Copier/Coller
* ? Duplication d'une séléction multiple (ou supprimer la duplication ?)
* BlackBox : agrandir quand il y a trop de fils => composants de taille variable
* ? Afficher des infos sur le composant hovered ou sélectionné
* Outil déplacement : flèches directionelles
* ? Créer une blackbox à partir de la selection
* Raccourcis pour les créations de composants (chiffres ?)
* Pouvoir connecter un même fil plusieurs fois sur un composant
* Curseur : changer les ciseaux ![Red](Wiring/Content/WireNodeOn.png)
* Fixer le stackoverflow

#### Eventuellement, si j'ai le courage :
* Modes de mise à jour : tout mettre à jour, normal, aléatoire
* Bug : le double-clic à l'ouverture fait aussi un clic sur l'application
* Ajustement auto de la Camera
* Personnalisation des BlackBox : image
* et/ou : Certains noms donnent des images aux BlackBox
* Angle des composants
* Menu de simulation (pas d'édition possible, possibilité de cacher des composants/fils, performances accrues ?)
* Faire des fils personnalisés : nodes où on veut
* OU : Fils à angle droit
* OU : meilleur algo de render des fils
* Historique des modifications => Annuler, Refaire...
* Menu clic droit
* Effets sonores + animations
* Composant `Random` : à chaque impulsion, renvoie  aléatoirement 1 ou 0 jusqu'à la fin de l'impulsion
* Selection au lasso
* "Librarie standard" de blackboxes utiles (and, add, mux...) ![Red](Wiring/Content/WireNodeOn.png)
* Pouvoir transformer les blackbox en bitmap pour gagner en efficacité
* ET/OU : Composant `Bitmap`
* Fichier de langue => traduction ![Red](Wiring/Content/WireNodeOn.png)

---

## Paramètres...

#### Ouverture

[ ] Ignorer les avertissements<br/>
(?) Améliore les performances durant l'ouverture, déconseillé si vous éditez le code de vos schematics

[ ] Ouvrir dans une nouvelle boîte noire

#### Enregistrement

[ ] Ne pas enregistrer les boîtes noires <br/>
(?) Il faudra les enregistrer séparément, sous leur nom actuel

[ ] Optimiser la taille du fichier<br/>
(?) La lisibilité du code en sera réduite

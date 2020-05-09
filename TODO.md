## Liste de trucs à faire : Projet Wiring


#### Relecture et Nettoyage du projet :
* Editor
* InputManager
* SchemReader
* SchemWriter

#### Documentation :
* Les outils : S, H/Z
* Les fils (C)
* Les différents composants

#### A faire maintenant :
* Système de delay : vérifier les bugs / rendre plus propre ?
* Diode : séparer clairement les 3 modes (ou en fusionner 2)
* Diode : gérer proprement le passage entre ces états
* Changer le nom du projet
* Mettre sur github et supprimer le repo gitlab

#### A faire plus tard :
* Modes de mise à jour : tout mettre à jour, normal, aléatoire
* Bug : affichage du fil allumé pdt la création pour une blackbox sans plugs
* Bug : annuler un enregistrement sauvegarde quand même un chemin
* Pouvoir connecter un même fil plusieurs fois sur un composant
* Gros refactoring : séparer les fils d'input et d'output dans les composants ?
* Revoir de système de tools : boutons pour chaque, boolean toogle, tool Wire à partir d'un plug
* Cursor création de fil
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
* Fichier de settings (settings d'enregistrement + couleurs de texte, tooltip...)

#### Eventuellement, si j'ai le courage :
* Bug : le double-clic à l'ouverture fait aussi un clic sur l'application
* Ajustement auto de la Camera
* Personnalisation des BlackBox : image
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
(?) Améliore les performances durant l'ouverture, déconseillé si vous éditez le code de vos schematics

[ ] Ouvrir dans une nouvelle boîte noire

#### Enregistrement

[ ] Ne pas enregistrer les boîtes noires <br/>
(?) Il faudra les enregistrer séparément, sous leur nom actuel

[ ] Optimiser la taille du fichier<br/>
(?) La lisibilité du code en sera réduite

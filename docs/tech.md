# Documentation technique

## Structure des fichiers

- `Assets/` : les différentes ressources du jeu (images, polices, niveaux)
- `Sources/` : le code source du jeu
  - `Toolbox/` : utilitaires réutilisables pour d'autres projets
  - `Gameplay/` : tout ce qui est propre à l'écran de jeu
  - `Editor/` : tout ce qui est propre à l'éditeur de niveaux
  - `Levels/` : tout ce qui touche aux niveaux (gestion des fichiers, parsing, etc.)

## Gestion de la mémoire

Dans ce jeu, j'ai besoin de créer de nouveaux objets à chaque niveau (Entity, EntityView)
et à chaque frame (Editor Widgets).

### Classes vs Structs

J'ai remarqué qu'en instanciant des classes, derrière le `new` se cache une allocation
mémoire sur la heap. En utilisant un profiler, on peut voir que la mémoire monte
constamment, mais le garbage collector ne passe pas.

Pour résoudre ce problème, j'utilise des structs dès que possible.

### Object Pool

Mais parfois, j'ai besoin de modifier les données d'une instance et une classe s'avère
plus pratique qu'une struct (comme les entités qui se déplacent). Dans le cas où j'ai
plusieurs objets du même type à créer fréquemment, j'ai recours à un Object Pool.

Au lieu d'instantier de nouveaux objets tout le temps, dès que je n'ai plus besoin d'un
objet, je le mets dans une stack prêt à être réutilisé. Donc, je ne crée des objets que
lorsque la pile est vide.

## Découplage du code

### Séparation mécaniques et UI

Pour ce projet et suite à des recommandations de
[développeurs](https://www.youtube.com/watch?v=Zzo5JTY8zjg)
[chevronnés](https://www.youtube.com/watch?v=drCnFueS4og),
je me suis fixé comme contrainte de séparer le code concernant les mécaniques du jeu
de celui pour l'interface utilisateur.

Bien que compliqué à appréhender au premier abord, je trouve cette technique très
intéressante, car elle permet de modifier le visuel sans risquer de casser les règles du
jeu. De plus, si je viens à travailler en équipe, il se peut que ce soit différentes
personnes qui s'occupent de chaque partie.

### Service Locator

Pour ce projet, j'ai utilisé le pattern Service Locator pour la gestion :
- des scènes,
- des images,
- des polices de caractères.

Ce design pattern est utilisé pour 2 choses :
1. Regrouper des éléments d'un même domaine au sein d'un dictionnaire
pour y faire appel n'importe où dans le code (comparable à une variable globale).
2. Enregistrer plusieurs services au sein d'un locator et pouvoir désigner lequel est
actif. C'est le cas pour les scènes de jeu : j'enregistre mes scènes puis je dis sur
laquelle je veux aller et la boucle de jeu s'exécute sur la scène active.

## Algorithmes de grilles

### Level parsing

Dans ce jeu, les niveaux sont enregistrés dans des fichiers. Un niveau a plusieurs layers.
Chaque layer est représenté par une grille :
```
:layers
11111111111
11111111111
11111111111
---
00050004000
00050005030
00040005000
---
00000000000
02000000000
00000000000
```
À la base, j'avais regardé pour utiliser le parseur natif JSON de C#, mais comme il fallait
écrire une fonction de parsing spécifique et qu'il attend un format de classe spéciale pour
désérialiser, j'ai préféré faire mon propre format et mon propre parseur de fichiers pour
les niveaux. Ce format est compact et permet assez facilement de visualiser et de modifier
son contenu.

### Gestion des déplacements

Lorsque dans le jeu on arrive sur un niveau, je crée un tableau 2D de toutes les entités.
Certaines peuvent se trouver sur la même cellule (car sur différents layers). Il est donc
de type `List<Entity>[,]`.

Lorsque l'utilisateur cherche à faire un déplacement de son personnage, je regarde la
cellule qu'il vise et les entités qui s'y trouvent et pour celles déplaçables je regarde
si elles peuvent bouger.

Si la résolution de tous les déplacements est OK, les changements sont appliqués au
tableau et la partie UI est informée des déplacements visuels à effectuer.

### Agrandissement de la grille

Les niveaux sont composés de chiffres représentant un type d'entité (personnage, caisse,
sortie, etc.). Comme on l'a vu ils sont enregistrés sous forme de grille qui est remplie
de 0 là où il n'y a pas d'entités. Ils occupent leur taille minimale, c'est-à-dire que
sur chaque "bord" de la grille, on retrouve au moins un chiffre différent de 0.

Dans ce jeu on peut déplacer des caisses. Si on en fait sortir une de la grille, elle
s'agrandit en fonction.

### Grille "infinie"

Dans l'éditeur de niveaux, j'utilise un dictionnaire pour enregistrer les entités plutôt
qu'un tableau ce qui a 2 avantages :
- l'agrandissement et le rétrécissement d'un tableau 2D est coûteux et il faudrait le faire
constamment,
- ça permet d'avoir visuellement un effet de grille infinie : on peut se déplacer assez
librement pour ajouter des entités.

Lorsqu'on enregistre un niveau, c'est à ce moment qu'on transforme le dictionnaire en 
tableau en regardant les positions des entités les plus éloignées.

## Concepts intéressants

### Éditeur intégré

La plus grande partie du développement a été consacrée à l'éditeur de niveaux.

Pourquoi créer un éditeur plutôt que d'utiliser un logiciel tiers comme Tiled ou LDTK ?
Je voulais pouvoir créer et tester mes niveaux instantanément sans devoir faire des
allers-retours avec d'autres interfaces.

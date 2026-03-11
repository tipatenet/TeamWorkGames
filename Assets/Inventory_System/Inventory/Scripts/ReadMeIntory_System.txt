Attention : Il faut récupérer à la main le scriptableObject ItemVide est le placer sur le script interact dans la variable objet vide

COMMENT SETUP INVENTORY_SYSTEM ?

Ajouter sur le Player les scripts Inventory_System, Interact et InspactItem.

1- Dans le script Inventory_System remplir la variable Item_Container et glisser L'item_Container du canvas sur cette varaible.

2- Dans le script Interact remplir la variable Cam et glissant la main camera dedans.

3- Dans le Script InspactItem remplir la variable Hold Position qui correspond a la 
position ou l'objet sera placer devant le joueur HoldPosition de base dans la scene d'exemple c'est l'enfant de la main cam.



Chaque Item a des parametre qu'il faut remplir dans son scriptable object :

COMMENT SETUP UN ITEM ?

1- Créer sont scriptableObject (Asset/Create/Item/Item_ScriptableObject), lui donner un nom unique en lien avec l'objet. 

2- Rentrer toute c'est information (Nom, sauf pour le goItem).

3- Créer une Prefab des l'objet avec le script "Item" dessus et donner dans la variable info le scriptableObject.

4- Sur ce préfab rajouter un RigidBody en constraints (toutes les rotations et les positions sauf la position Y).

5- Compléter les infos du scriptableObject en rajouter pour goItem la prefab de l'object avec le script et le RigidBody

6- Rajouter le tage item sur le prefab de l'item

Et après c'est bon tu peux placer la prefab de l'item (avec le script et rigidbody) dans la scene et sa marche !



Si cela marche pas prendre exemple sur la scene d'exemple ou me demander en MP
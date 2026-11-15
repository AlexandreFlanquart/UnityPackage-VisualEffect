## 🎨 Shaders

Ce guide s'adresse à un développeur Unity/C# qui n'a jamais écrit de shader. Il explique les bases, montre comment piloter un shader avec le package, puis construit six effets pas à pas avec Shader Graph (URP).

## Introduction

Un **shader** est un programme qui s'exécute sur le GPU pour dessiner un objet. Il a deux étapes principales :

- le **vertex stage** (étape des sommets) place chaque sommet du mesh à l'écran ;
- le **fragment stage** (étape des fragments) calcule la couleur de chaque pixel couvert par l'objet.

Un **material** est un shader plus des valeurs de propriétés (texture, couleur, nombres). Plusieurs materials peuvent partager le même shader avec des valeurs différentes.

**Shader Graph** est l'éditeur visuel de shaders (nœuds reliés par des liens). C'est l'approche recommandée par Unity pour URP : pas de HLSL à écrire, aperçu en direct, compatible avec les cibles de rendu d'URP.

Le package fournit aussi un shader écrit à la main en HLSL, `MyUnityPackage/Visual Effect/Sprite Effects` ([`SpriteEffects.shader`](../Runtime/Shaders/SpriteEffects.shader)). Il est entièrement commenté et se lit comme un premier exemple de HLSL : les mêmes effets (flash, dissolve) y sont écrits en code plutôt qu'en nœuds.

## Notions à connaître avant de commencer

### Créer un graph

**Project window > Create > Shader Graph > URP >** puis choisir la cible :

| Cible | Quand la choisir |
|-------|------------------|
| Lit Shader Graph | Objet 3D éclairé (lumières, ombres, PBR) |
| Unlit Shader Graph | Objet 3D sans éclairage |
| Sprite Unlit Shader Graph | Sprite 2D sans éclairage 2D |
| Sprite Lit Shader Graph | Sprite 2D éclairé par les lumières 2D |
| Canvas Shader Graph | Élément uGUI (Image, RawImage...) |
| Fullscreen Shader Graph | Post-process plein écran |

Les libellés exacts peuvent varier selon la version de Unity 6.

### Propriétés du Blackboard

Les propriétés se créent dans le **Blackboard** (bouton **+**). Chaque propriété a deux noms :

- le **nom affiché** (Name), lisible, montré dans l'Inspector du material ;
- le **Reference name**, l'identifiant utilisé par le code (`_FlashAmount`). Il se règle dans **Graph Inspector > Node Settings** quand la propriété est sélectionnée dans le Blackboard.

**Les scripts utilisent toujours le Reference name**, jamais le nom affiché. Par défaut Shader Graph en génère un à partir du nom (par exemple `_Flash_Amount`) : vérifiez-le et corrigez-le.

Les composants du package attendent exactement `_FlashAmount`, `_FlashColor` et `_DissolveAmount`. Si votre graph utilise ces Reference names, le même composant (`HitFlashEffect`, `DissolveEffect`) pilote n'importe quel shader, le vôtre comme celui du package.

Une propriété doit être **exposée** (case *Exposed* dans Node Settings) pour apparaître dans le material et pouvoir être pilotée par script.

### Master Stack

La sortie du graph est le **Master Stack**, une liste de blocs :

- **Vertex** : `Position` (et `Normal`, `Tangent`) ;
- **Fragment** : `Base Color`, `Alpha`, et sur la cible Lit `Emission`, `Metallic`, `Smoothness`, etc. ;
- `Alpha Clip Threshold` apparaît dans Fragment quand **Alpha Clipping** est activé dans **Graph Settings** (Graph Inspector).

Le Graph Inspector (bouton en haut à droite du graph) contient les onglets Node Settings et Graph Settings.

### Enregistrer et créer un material

Les modifications ne s'appliquent qu'après **Save Asset** (barre du graph). Pour créer un material : clic droit sur le graph dans la Project window > **Create > Material** (ou **Create > Material** puis choisir le shader dans l'Inspector).

## Piloter un shader avec le package

### `MaterialFloatEffect`

[`MaterialFloatEffect`](../Runtime/Core/Scripts/Effects/MaterialFloatEffect.cs) anime **n'importe quelle propriété float** d'un shader, désignée par son Reference name.

| Champ | Rôle |
|-------|------|
| `Renderers` | Renderers à animer (par défaut, tous ceux de l'objet et de ses enfants) |
| `PropertyName` | Reference name de la propriété (`_FlashAmount`...), pas le nom affiché |
| `From` / `To` | Valeur de départ et d'arrivée |
| `Ease` | Courbe d'interpolation |

S'y ajoutent les champs communs de `VisualEffectBehaviour` (`PlayOnEnable`, `Duration`, `Delay`, `Loops`, `LoopType`, `UseUnscaledTime`, `OnComplete`) et les boutons **Play** / **Stop** de l'Inspector, utilisables aussi en Edit Mode. Voir [Effects.md](Effects.md).

### Les presets

| Composant | Propriété | De / vers | Durée |
|-----------|-----------|-----------|-------|
| `HitFlashEffect` | `_FlashAmount` | 1 vers 0 | 0,15 s |
| `DissolveEffect` | `_DissolveAmount` | 0 vers 1 | 1 s |

Ce sont de simples valeurs par défaut de `MaterialFloatEffect` : on peut les modifier dans l'Inspector, ou utiliser `MaterialFloatEffect` directement pour une autre propriété (`_RimAmount`, `_OutlineWidth`...).

### Déclencher

```csharp
[SerializeField] private HitFlashEffect _flash;

void OnHit()
{
    _flash.Play();
}
```

Ou depuis un `UnityEvent`, en choisissant la méthode `Play`. À la fin, la valeur finale reste appliquée jusqu'au prochain `Play()` ou à `Stop()`.

### Compromis à connaître

Les valeurs passent par un `MaterialPropertyBlock` : l'asset material n'est **jamais modifié** et aucune instance de material n'est créée. En contrepartie, un renderer qui porte un property block **n'est pas batché par le SRP Batcher** tant que l'effet le détient. Le block est retiré quand l'effet est réinitialisé (`Stop()` ou nouveau `Play()`), et le renderer redevient batchable.

Le composant cible des composants `Renderer` (`MeshRenderer`, `SkinnedMeshRenderer`, `SpriteRenderer`). Les éléments uGUI (`Graphic`) ne sont pas pris en charge pour l'instant : prévu dans une version ultérieure.

## Utiliser le shader fourni (sprites)

1. **Create > Material**, puis dans l'Inspector choisir le shader `MyUnityPackage/Visual Effect/Sprite Effects`.
2. Assigner le material au `SpriteRenderer` (champ *Material*).
3. Ajouter `HitFlashEffect` ou `DissolveEffect` sur le même objet (**Add Component > MyUnityPackage > Visual Effect**).
4. Sélectionner l'objet et cliquer sur **Play** dans l'Inspector du composant.

Propriétés du shader :

| Reference name | Type | Défaut | Rôle |
|----------------|------|--------|------|
| `_MainTex` | Texture 2D | white | Texture du sprite (remplie automatiquement par le `SpriteRenderer`) |
| `_Color` | Color | (1, 1, 1, 1) | Teinte multipliée |
| `_FlashColor` | Color (HDR) | (1, 1, 1, 1) | Couleur du flash |
| `_FlashAmount` | Range 0-1 | 0 | Intensité du flash (0 = aucun, 1 = couleur pleine) |
| `_DissolveAmount` | Range 0-1 | 0 | Progression du dissolve (0 = visible, 1 = disparu) |
| `_DissolveScale` | Float | 20 | Échelle du bruit |
| `_DissolveEdgeWidth` | Range 0-0,3 | 0,05 | Largeur du bord lumineux |
| `_DissolveEdgeColor` | Color (HDR) | (2, 0,8, 0,2, 1) | Couleur du bord |

Ce shader ne fonctionne qu'avec un `SpriteRenderer` : il repose sur les données propres aux sprites (flip, couleur du renderer). Il n'est pas prévu pour un `MeshRenderer`.

## Tutoriels Shader Graph

Progression conseillée : 5.1 à 5.3 d'abord, puis le reste dans l'ordre qui vous intéresse. Pour chaque tutoriel, créez le graph, sauvegardez (**Save Asset**), créez un material et assignez-le à un objet.

Rappel : nommez les Reference names exactement comme indiqué. Pour ajouter un nœud : clic droit dans le graph > **Create Node**, puis tapez son nom.

### 5.1 Hit flash

**Objectif** : faire flasher un objet d'une couleur unie, comme `HitFlashEffect` le fait avec le shader fourni.
**Notion** : propriétés et `Lerp`.
**Cible** : Lit ou Sprite Unlit.

1. Créer la propriété **Texture2D** : Reference name `_BaseMap` (cible Lit) ou `_MainTex` (cible Sprite : obligatoire, c'est la texture que `SpriteRenderer` remplit).
2. Créer la propriété **Color**, mode **HDR**, Reference name `_FlashColor` (par exemple blanc).
3. Créer la propriété **Float**, Reference name `_FlashAmount`, type d'affichage **Slider** de 0 à 1, défaut 0.
4. Glisser les trois propriétés dans le graph. Ajouter un nœud **Sample Texture 2D** et relier la texture à son entrée *Texture*.
5. Ajouter un nœud **Lerp** : entrée **A** = sortie **RGBA** du Sample Texture 2D (seul le RGB est utilisé, relier à A de type Vector3 convertit automatiquement), entrée **B** = `_FlashColor`, entrée **T** = `_FlashAmount`.
6. Relier la sortie du Lerp à **Base Color**, et la sortie **A** du Sample Texture 2D au bloc **Alpha** (pour garder la silhouette du sprite).

`Lerp(A, B, T)` donne A quand T = 0 et B quand T = 1. Le flash est donc une interpolation de la couleur de la texture vers la couleur de flash.

**Piloter** : `HitFlashEffect` (anime `_FlashAmount` de 1 à 0).

### 5.2 Défilement d'UV et pulse

**Objectif** : faire défiler une texture, et faire pulser une couleur.
**Notion** : UV et `Time`. Aucun script nécessaire.

Défilement :

1. Créer la propriété **Vector2** `_ScrollSpeed` (par exemple (0,2 ; 0)).
2. Nœud **Time** : sortie **Time** (secondes). Nœud **Multiply** : **Time** × `_ScrollSpeed`.
3. Nœud **Tiling And Offset** : entrée **Offset** = résultat du Multiply (l'entrée **UV** utilise par défaut le canal UV0 du mesh, vous pouvez aussi y relier un nœud **UV**).
4. Relier la sortie de **Tiling And Offset** à l'entrée **UV** du **Sample Texture 2D**. Pour un défilement continu, la texture doit être en Wrap Mode **Repeat**.

Pulse :

1. Nœud **Time** : sortie **Sine Time** (valeur de sinus du temps, entre -1 et 1).
2. Nœud **Remap** : In Min Max = (-1, 1), Out Min Max = (0, 1).
3. **Multiply** : le résultat × une couleur HDR (propriété **Color**), puis relier à **Emission** (Lit) ou l'ajouter à **Base Color** avec un nœud **Add** (Unlit).

La couleur oscille en continu entre noir et pleine intensité.

### 5.3 Dissolve

**Objectif** : faire disparaître un objet par taches, avec un bord lumineux.
**Notions** : bruit, `Step`, alpha clipping.
**Cible** : Lit ou Sprite Unlit.

1. Propriétés : `_DissolveAmount` (Float, slider 0-1, défaut 0), `_DissolveEdgeWidth` (Float, slider 0-0,3, défaut 0,05), `_DissolveEdgeColor` (Color, HDR).
2. **Graph Settings > Alpha Clipping** : activer. Le bloc **Alpha Clip Threshold** apparaît dans Fragment (le libellé peut varier selon la version).
3. Nœud **Simple Noise** : **Scale** ≈ 30. Relier sa sortie au bloc **Alpha**.
4. Relier `_DissolveAmount` au bloc **Alpha Clip Threshold**.
5. Bord : nœud **Add** : `_DissolveAmount` + `_DissolveEdgeWidth`. Nœud **Step** : **Edge** = ce résultat, **In** = sortie du Simple Noise.
6. Relier le **Step** à un **One Minus**, puis **Multiply** par `_DissolveEdgeColor`.
7. Envoyer le résultat dans **Emission** (Lit) ou l'ajouter à la couleur de base avec **Add** puis **Base Color** (Unlit).
8. Pour garder l'aspect de l'objet, brancher aussi la texture dans **Base Color** comme en 5.1.

Pourquoi ça marche : avec l'alpha clipping, tout pixel dont l'alpha (ici la valeur du bruit) est inférieur au seuil est **jeté** ; en montant `_DissolveAmount` de 0 à 1, de plus en plus de pixels passent sous le seuil. Le `Step` isole la bande de pixels juste au-dessus du seuil : c'est le bord lumineux.

Remarques : sur une surface Opaque, l'alpha ne sert qu'au clipping (l'objet reste opaque). Sur une cible Sprite, l'alpha reste utilisé pour la transparence : la valeur du bruit rend alors les bords semi-transparents. Dans ce cas, remplacer l'entrée Alpha par un **Step** (Edge = `_DissolveAmount`, In = bruit), ce qui donne un alpha de 0 ou 1. À `_DissolveAmount` = 0, une fine bande de pixels sombres du bruit peut déjà briller : ajuster `_DissolveEdgeWidth` ou le multiplier par `_DissolveAmount`.

**Piloter** : `DissolveEffect` (anime `_DissolveAmount` de 0 à 1).

### 5.4 Rim light (Fresnel)

**Objectif** : un liseré lumineux sur les bords de l'objet.
**Notion** : normale et direction de vue (le Fresnel est fort quand la surface est vue de biais).
**Cible** : Lit.

1. Propriétés : `_RimPower` (Float, défaut 3), `_RimColor` (Color, HDR), `_RimAmount` (Float, slider 0-1, défaut 1).
2. Nœud **Fresnel Effect** : **Power** = `_RimPower`. (Ses entrées *Normal* et *View Dir* utilisent par défaut la normale et la vue du pixel.)
3. **Multiply** : sortie du Fresnel × `_RimColor`.
4. **Multiply** : le résultat × `_RimAmount`.
5. Relier à **Emission**.

Une puissance plus grande resserre le liseré sur les bords.

**Piloter** : `MaterialFloatEffect` avec `PropertyName` = `_RimAmount` (ou `_RimPower`), From / To au choix, par exemple pour un objet sélectionné qui s'illumine.

### 5.5 Outline (coque inversée)

**Objectif** : un contour autour d'un mesh 3D.
**Notion** : vertex stage (déplacer les sommets le long de leur normale).
**Cible** : Unlit.

Principe : un **second material** sur le même mesh dessine une version gonflée, dont on ne voit que les faces arrière ; elle déborde autour de l'objet.

1. Propriétés : `_OutlineWidth` (Float, défaut 0,02), `_OutlineColor` (Color).
2. **Graph Settings > Render Face : Back** (le libellé peut varier selon la version).
3. Nœud **Position** (Space : Object) et nœud **Normal Vector** (Space : Object).
4. **Multiply** : **Normal Vector** × `_OutlineWidth`. **Add** : **Position** + ce résultat.
5. Relier le **Add** au bloc **Vertex > Position**.
6. `_OutlineColor` vers **Base Color**.
7. Sur le `MeshRenderer`, ajouter ce material comme **second élément** de la liste *Materials*. Sur un mesh à un seul sous-mesh, Unity redessine alors le mesh avec ce material ; l'éditeur peut afficher un avertissement sur le nombre de materials, sans conséquence ici.

Limites : il faut des **normales lissées** (sur un cube aux arêtes dures, les sommets sont dupliqués avec des normales différentes et le contour se déchire) ; la technique est limitée aux **meshes 3D**. Une approche plein écran, basée sur le buffer de profondeur ou de normales, est l'alternative plus robuste, mais plus complexe.

**Piloter** : `MaterialFloatEffect` avec `PropertyName` = `_OutlineWidth`. Attention : le `MaterialPropertyBlock` s'applique à tous les materials du renderer, ce qui convient ici car seul le second material porte cette propriété.

### 5.6 Effet plein écran (vignette)

**Objectif** : assombrir les bords de l'écran.
**Notion** : post-process.

1. **Create > Shader Graph > URP > Fullscreen Shader Graph**.
2. Nœud **URP Sample Buffer** : **Source Buffer** = **BlitSource**. Sa sortie donne la couleur de l'écran.
3. Nœud **Screen Position** (mode **Default**) : prendre les coordonnées x, y (0 à 1 sur l'écran).
4. Nœud **Distance** : **A** = Screen Position, **B** = un nœud **Vector2** de valeur (0,5 ; 0,5). C'est la distance au centre.
5. Nœud **Smoothstep** : **Edge1** et **Edge2** (par exemple 0,3 et 0,8), **In** = la distance. Puis **One Minus**.
6. **Multiply** : la couleur de l'écran × le résultat. Relier à **Base Color**.
7. Créer un material avec ce graph.
8. Sur l'asset **URP Renderer** : **Add Renderer Feature > Full Screen Pass Renderer Feature**, assigner le material dans *Pass Material*, injection point **After Rendering Post Processing**.

Ce material n'est porté par aucun `Renderer` : **`MaterialFloatEffect` ne peut pas le piloter pour l'instant** (prévu dans une version ultérieure). Modifiez ses valeurs à la main ou via `Material.SetFloat` sur l'asset.

## Aller plus loin

- Les samples Unity de Shader Graph, importables depuis **Package Manager > Shader Graph > Samples** : *Production Ready Shaders*, *Feature Examples*, *UGUI Shaders*. [Documentation](https://docs.unity3d.com/Packages/com.unity.shadergraph@17.0/manual/ShaderGraph-Samples.html).
- L'e-book gratuit d'Unity *Create popular shaders and visual effects in URP (Unity 6 edition)* : niveau intermédiaire / avancé, à lire après 5.1 à 5.3. [Lien](https://unity.com/resources/create-shaders-visual-effects-urp-unity-6).
- La page du manuel *Full Screen Pass Renderer Feature*. [Lien](https://docs.unity3d.com/Manual/urp/renderer-features/renderer-feature-full-screen-pass.html).

## Dépannage

| Symptôme | Cause probable |
|----------|----------------|
| Material rose | Le shader ne compile pas, ou URP n'est pas le pipeline actif (**Project Settings > Graphics** et **Quality**) |
| Rien ne se passe au `Play` | Le `PropertyName` ne correspond pas au Reference name (vérifier dans Node Settings, pas dans le nom affiché), ou la propriété n'est pas exposée |
| Sprite invisible avec un graph Sprite personnalisé | La propriété texture n'a pas le Reference name `_MainTex` |
| L'objet reste modifié après l'effet | La valeur finale reste appliquée jusqu'au prochain `Play()` : appeler `Stop()` pour retirer le block et restaurer le material |

*Le shader et les tutoriels ont été écrits sans accès à un éditeur Unity : si un libellé de menu diffère dans votre version, merci de le signaler.*

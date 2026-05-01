# Tokens de diseño

Referencia de variables CSS que definen la identidad visual del sistema.

## Paleta de colores

| Token | Nombre | Hex | Uso |
|-------|--------|-----|-----|
| `--lmd-azul-duque` | Azul Duque | `#0F1B2D` | Fondo de navegación, textos principales, fondos oscuros |
| `--lmd-dorado-real` | Dorado Real | `#C9A24E` | Acentos, botones primarios, enlaces, elementos de marca |
| `--lmd-marfil` | Marfil | `#F7F4EC` | Fondo general de la aplicación |
| `--lmd-verde-salvia` | Verde Salvia | `#4E6B58` | Estados de éxito, confirmaciones |
| `--lmd-terracota` | Terracota | `#C75A3C` | Alertas, errores, estados de peligro |
| `--lmd-gris-piedra` | Gris Piedra | `#6B6F76` | Textos secundarios, elementos neutros |

### Variantes derivadas

| Token | Hex | Uso |
|-------|-----|-----|
| `--lmd-azul-suave` | `#1a2a42` | Hover sobre fondos azules |
| `--lmd-dorado-claro` | `#d4b56a` | Hover sobre elementos dorados claros |
| `--lmd-dorado-hover` | `#b8912e` | Hover sobre botones dorados |
| `--lmd-marfil-oscuro` | `#ebe6d8` | Bordes suaves, separadores |

## Tipografía

| Rol | Fuente | Peso | Token |
|-----|--------|------|-------|
| Títulos | Cinzel | SemiBold (600) | `--lmd-fuente-titulo` |
| Cuerpo | Montserrat | Regular (400) | `--lmd-fuente-cuerpo` |
| Énfasis | Montserrat | SemiBold (600) | `--lmd-peso-enfasis` |

### Escala tipográfica

| Token | Tamaño |
|-------|--------|
| `--lmd-texto-xs` | 0.75rem |
| `--lmd-texto-sm` | 0.875rem |
| `--lmd-texto-base` | 1rem |
| `--lmd-texto-lg` | 1.125rem |
| `--lmd-texto-xl` | 1.25rem |
| `--lmd-texto-2xl` | 1.5rem |
| `--lmd-texto-3xl` | 1.875rem |
| `--lmd-texto-4xl` | 2.25rem |

## Espaciado

| Token | Valor |
|-------|-------|
| `--lmd-espacio-xs` | 0.25rem |
| `--lmd-espacio-sm` | 0.5rem |
| `--lmd-espacio-md` | 1rem |
| `--lmd-espacio-lg` | 1.5rem |
| `--lmd-espacio-xl` | 2rem |
| `--lmd-espacio-2xl` | 3rem |
| `--lmd-espacio-3xl` | 4rem |

## Bordes y sombras

| Token | Valor |
|-------|-------|
| `--lmd-radio-sm` | 0.25rem |
| `--lmd-radio-md` | 0.5rem |
| `--lmd-radio-lg` | 0.75rem |
| `--lmd-sombra-sm` | `0 1px 3px rgba(15,27,45,.08)` |
| `--lmd-sombra-md` | `0 4px 12px rgba(15,27,45,.10)` |
| `--lmd-sombra-lg` | `0 8px 24px rgba(15,27,45,.12)` |

## Usos del logotipo

| Variante | Archivo | Contexto |
|----------|---------|----------|
| Principal | `logo-principal.png` | Web, aplicaciones, documentos corporativos |
| Color sólido | `logo-solido.png` | Fondos oscuros, alto contraste |
| Monocromo | `logo-monocromo.png` | Impresiones blanco/negro |
| Símbolo | `logo-simbolo.png` | Favicon, icono de app, navegación |
| Icono | `logo-icono.png` | Solo la marca gráfica sin texto |
| Completo | `logo-completo.png` | Logo con tipografía completa |

## Componentes con marca

### Navegación (`.lmd-navbar`)
- Fondo: Azul Duque
- Marca: logo-simbolo + texto en Cinzel Dorado
- Enlaces: Montserrat SemiBold, mayúsculas, blancos con hover dorado

### Tarjetas (`.lmd-tarjeta`)
- Fondo blanco, borde marfil oscuro, sombra suave
- Hover: elevación sutil

### Botones (`.lmd-btn-primario` / `.lmd-btn-contorno`)
- Primario: fondo dorado, texto azul
- Contorno: borde dorado, fondo transparente

### Footer (`.lmd-footer`)
- Fondo Azul Duque, texto marfil, enlaces dorados

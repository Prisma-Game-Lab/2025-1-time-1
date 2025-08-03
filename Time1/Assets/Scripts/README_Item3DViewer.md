# Sistema de Visualização de Itens - Item3DViewer

## Visão Geral
O sistema Item3DViewer foi otimizado para mostrar itens 3D após a vitória em uma batalha, com transições suaves e interface intuitiva.

## Configuração no Unity

### 1. Estrutura do GameObject
```
Item3DViewer (GameObject principal)
├── Canvas Group (para transições)
├── Item3DViewer (Script)
├── Item3DViewerSetup (Script opcional)
├── Viewer (Canvas filho)
│   ├── RawImage (plano de fundo)
│   │   └── Item3DHolder (onde ficam os itens 3D)
│   │       ├── bombom
│   │       ├── calculadora
│   │       └── outros itens...
│   ├── RewardText (TextMeshProUGUI)
│   └── ContinueButton (Button)
```

### 2. Configuração Manual

1. **No GameObject Item3DViewer:**
   - Adicione o script `Item3DViewer`
   - Configure o `Item Holder` para apontar para o `Item3DHolder`
   - Configure `Rotation Speed` (padrão: 100)
   - Configure `View Duration` (padrão: 5)

2. **No script Item3DViewer:**
   - Arraste o `RewardText` (TextMeshProUGUI) para o campo correspondente
   - Arraste o `ContinueButton` (Button) para o campo correspondente
   - O `CanvasGroup` será adicionado automaticamente se não existir

3. **No BattleManager:**
   - Arraste o GameObject `Item3DViewer` para o campo `Item Viewer`

### 3. Configuração Automática (Recomendado)

1. Adicione o script `Item3DViewerSetup` ao GameObject `Item3DViewer`
2. Clique com botão direito no script e selecione "Configurar Automaticamente"
3. Isso irá procurar e configurar automaticamente todas as referências

### 4. Teste

1. Clique com botão direito no script `Item3DViewerSetup`
2. Selecione "Testar Visualização"
3. O item deve aparecer com transição suave

## Funcionalidades

### Transições Suaves
- **Fade In:** O viewer aparece gradualmente (0.5s)
- **Fade Out:** O viewer desaparece gradualmente (0.5s) após clicar em continuar

### Interface
- **Texto de Recompensa:** "Você ganhou um item!!"
- **Botão Continuar:** Aparece após 1 segundo
- **Rotação do Item:** O item 3D gira continuamente

### Integração com BattleManager
- Ativado automaticamente após vitória na batalha
- Transição automática para a próxima cena após visualização

## Personalização

### Mudar Item Exibido
No BattleManager, método `ShowRewardItem()`:
```csharp
itemViewer.ShowItem("nome_do_item");
```

### Mudar Texto de Recompensa
No script Item3DViewer, método `Awake()`:
```csharp
rewardText.text = "Seu texto personalizado aqui!";
```

### Ajustar Velocidade de Transição
No script Item3DViewer, métodos `FadeInTransition()` e `FadeOutTransition()`:
```csharp
float duration = 0.5f; // Ajuste este valor
```

## Troubleshooting

### Item não aparece
- Verifique se o `Item Holder` está configurado corretamente
- Confirme se o item existe como filho do `Item3DHolder`
- Verifique se o GameObject do item está ativo

### Transição não funciona
- Verifique se o `CanvasGroup` está presente
- Confirme se o `Canvas` está configurado corretamente
- Verifique se não há conflitos com outros scripts

### Botão não responde
- Verifique se o `ContinueButton` está configurado
- Confirme se o botão tem um `Button` component
- Verifique se o evento `onClick` está conectado

## Estrutura de Arquivos
- `Item3DViewer.cs` - Script principal
- `Item3DViewerSetup.cs` - Script de configuração automática
- `BattleManager.cs` - Integração com sistema de batalha 
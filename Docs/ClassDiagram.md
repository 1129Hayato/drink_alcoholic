# クラス図（初版）

## シーン構成

| シーン | 内容 |
|---|---|
| `Title` | スタート / 設定パネル |
| `MainGame` | 飲み会本編。ゲームオーバーは同シーン内のパネルで表示（「飲みなおす」＝シーン再読込） |

`AudioManager` のみ `DontDestroyOnLoad` でシーンをまたいで常駐します。

## クラス図

```mermaid
classDiagram
    direction TB

    %% ===== 共通 =====
    class AudioManager {
        <<Singleton / DontDestroyOnLoad>>
        +static Instance : AudioManager
        -bgmSource : AudioSource
        -seSource : AudioSource
        +BgmVolume : float
        +SeVolume : float
        +SetBgmVolume(v : float)
        +SetSeVolume(v : float)
        +PlayBgm(clip : AudioClip)
        +PlaySe(clip : AudioClip)
        -Load() / Save()  PlayerPrefs
    }

    class SceneLoader {
        <<static>>
        +LoadTitle()
        +LoadMainGame()
    }

    class GameConfig {
        <<ScriptableObject>>
        +pourRateMlPerSec : float = 30
        +gaugeMax : float = 10
        +decayMinPerSec : float = 0
        +decayMaxPerSec : float = 2
        +decayNoiseFrequency : float = 0.3
        +gaugeVisibleSeconds : float = 10
    }

    %% ===== タイトル =====
    class TitleController {
        -settingsPanel : SettingsPanel
        +OnClickStart()
        +OnClickSettings()
    }

    class SettingsPanel {
        -bgmSlider : Slider
        -seSlider : Slider
        -closeButton : Button
        +Open()
        +Close()
        -OnBgmChanged(v : float)
        -OnSeChanged(v : float)
    }

    %% ===== 酒 =====
    class LiquorData {
        <<ScriptableObject>>
        +liquorName : string
        +volumeMl : float
        +alcoholPercent : float  例 5 = 5%
        +AlcoholRatio : float  alcoholPercent / 100
        +sprite : Sprite
        +color : Color  仮表示色
    }

    class Liquor {
        +Data : LiquorData
        +RemainingMl : float
        +IsEmpty : bool
        +Pour(requestMl : float) float
    }

    class LiquorFactory {
        -liquorTable : LiquorData[3]
        +CreateRandom() Liquor
    }

    %% ===== 登場人物 =====
    class PlayerController {
        <<右側：飲ませる人>>
        -factory : LiquorFactory
        +CurrentLiquor : Liquor
        +event OnBottleEmptied
        +TakeNewBottle()
        +PourTo(target : DrinkerTarget, mlPerSec : float, deltaTime : float)
    }

    class DrinkerTarget {
        <<左側：飲まされる人>>
        +Gauge : AlcoholGauge
        +Drink(ml : float, alcoholRatio : float)
    }

    class AlcoholGauge {
        <<純粋C#クラス>>
        +Current : float
        +Max : float
        -decayRate : IDecayRate
        +IsOverflow : bool
        +event OnChanged(float, float)
        +event OnOverflow()
        +Add(amount : float)
        +Tick(deltaTime : float)
    }

    class IDecayRate {
        <<interface>>
        +Evaluate(time : float) float
    }

    class PerlinDecayRate {
        -min : float
        -max : float
        -frequency : float
        -seed : float
        +Evaluate(time : float) float
    }

    class AlcoholGaugeView {
        -fillImage : Image
        -visibleTimer : float
        +Bind(gauge : AlcoholGauge)
        +ShowFor(seconds : float)
        -Refresh(current : float, max : float)
    }

    %% ===== 本編進行 =====
    class GameState {
        <<enumeration>>
        Ready
        Playing
        GameOver
    }

    class MainGameController {
        -config : GameConfig
        -player : PlayerController
        -target : DrinkerTarget
        -gaugeView : AlcoholGaugeView
        -score : ScoreCounter
        -hud : MainGameHud
        -gameOverPanel : GameOverPanel
        +State : GameState
        +StartParty()
        -Update()
        -HandleGameOver()
    }

    class ScoreCounter {
        <<純粋C#クラス>>
        +BottleCount : int
        +event OnChanged(int)
        +AddBottle()
        +Reset()
    }

    class PourButton {
        <<IPointerDownHandler / IPointerUpHandler>>
        +IsPressed : bool
    }

    class MainGameHud {
        -startButton : Button
        -pourButton : PourButton
        -scoreText : TMP_Text
        +event OnStartClicked
        +IsPouring : bool
        +SetScore(count : int)
        +SetState(state : GameState)
    }

    class GameOverPanel {
        -messageText : TMP_Text  「文化祭短縮…」
        -resultText : TMP_Text
        -retryButton : Button  「飲みなおす」
        -titleButton : Button
        +Show(bottleCount : int)
        +Hide()
    }

    %% ===== 関連 =====
    TitleController --> SettingsPanel
    TitleController ..> SceneLoader
    SettingsPanel ..> AudioManager

    LiquorFactory o-- "3" LiquorData
    LiquorFactory ..> Liquor : creates
    Liquor --> LiquorData

    PlayerController --> LiquorFactory
    PlayerController --> "0..1" Liquor : 手に持つ
    PlayerController ..> DrinkerTarget : 飲ませる
    DrinkerTarget *-- AlcoholGauge
    AlcoholGauge --> IDecayRate
    IDecayRate <|.. PerlinDecayRate
    AlcoholGaugeView ..> AlcoholGauge : observes

    MainGameController --> GameConfig
    MainGameController --> GameState
    MainGameController --> PlayerController
    MainGameController --> DrinkerTarget
    MainGameController --> AlcoholGaugeView
    MainGameController --> ScoreCounter
    MainGameController --> MainGameHud
    MainGameController --> GameOverPanel
    MainGameHud *-- PourButton
    GameOverPanel ..> SceneLoader
    MainGameController ..> AudioManager
```

## 主要ロジック（毎フレーム）

```
MainGameController.Update (State == Playing)
 ├─ if hud.IsPouring:
 │    player.PourTo(target, dt)
 │      ml = CurrentLiquor.Pour(config.pourRateMlPerSec * dt)   // 30ml/秒 固定
 │      target.Drink(ml, data.AlcoholRatio) → Gauge.Add(ml * ratio)
 │      if CurrentLiquor.IsEmpty → OnBottleEmptied → score.AddBottle() → TakeNewBottle()（ランダム）
 ├─ target.Gauge.Tick(dt)        // 減少量 = decayRate.Evaluate(経過時間) * dt（0未満にはしない）
 └─ Gauge.OnOverflow → HandleGameOver() → GameOverPanel.Show(score)
```

- ゲージ表示: `StartParty()` で `gaugeView.ShowFor(10)`、10秒後に非表示（値は内部で更新し続ける）。
- クリア条件なし。ゲームオーバーからのみ終了。

## ゲージ減少量（ランダム・連続）

毎秒の減少量を 0〜2 の範囲で**なめらかに**変化させるため、Perlinノイズを使います。

```csharp
public float Evaluate(float time)
{
    float n = Mathf.Clamp01(Mathf.PerlinNoise(time * frequency, seed)); // 0〜1 の連続値
    return Mathf.Lerp(min, max, n);                                     // 0〜2 /秒
}
```

- `seed` はゲーム開始時に `Random.Range(0f, 1000f)` で決めるので、毎回違う変化になります。
- `frequency` が小さいほどゆっくり変化します（0.3 なら数秒かけてうねる程度）。
- `Mathf.PerlinNoise` は範囲外の値をわずかに返すことがあるため `Clamp01` しています。

## 確定事項

1. アルコール度は **%** で持つ（`alcoholPercent = 5` → 加算時 `ml × 5 / 100`）。
2. 「飲ませる」は **長押し** で注ぎ続ける（`PourButton`）。
3. 減少量は **0〜2/秒のランダム値を連続的に変化**（Perlinノイズ）。
4. スコアは **1本飲み切った時点で +1**。
5. ゲージの長さは **キャラ画像の高さの80%** まで。
6. ゲージ上限は **10**（元仕様の50から変更）。

# 姘存潐璋冩牎鍙?路 Metasequoia Tuner

涓€涓粰 [姘存潐杈撳叆娉昡(https://msime.app/)锛圡etasequoia IME锛夌敤鐨?Windows 灏忓伐鍏凤細**鍙鍖栬皟鍊欓€夌獥鐨偆銆佸瓧浣撳拰缂栫爜鏄剧ず锛屾敼瀹屼竴閿噸鍚緭鍏ユ硶**銆?
涓嶇敤鍐嶆墜鍐?`skin.toml` / `config.toml`锛屼篃涓嶇敤姣忔鏀瑰畬鍘讳换鍔＄鐞嗗櫒閲屾潃杩涚▼銆?
![娣辫壊涓婚](screenshots/dark.png)

## 鍔熻兘

### 鐨偆鍙傛暟锛堝啓鍏?`skin.toml`锛?
- **闂磋窛涓庡竷灞€**锛氬€欓€夐棿璺?`item_gap`銆佸唴杈硅窛 `padding`銆佸€欓€夊彸鐣欑櫧銆佸簭鍙烽棿璺濄€佸€欓€夋渶灏忓搴︺€佸崱鐗囧渾瑙掋€佸崱鐗囨渶灏忓搴︺€佸唴瀹瑰乏鍐呯缉 `number_inset`
- **閫変腑绔栨潯**锛氬搴?`selected_bar_width`銆佹槸鍚︽樉绀?`show_selected_bar`銆佽创鍐?楠戣竟 `selected_bar_inside`
- **棰滆壊**锛氶€変腑鑳屾櫙 / 閫変腑鏂囧瓧 / 寮鸿皟鑹?/ 鎮仠鑹?/ 绔栨潯鑹诧紝娴呰壊娣辫壊鍚勪竴濂楋紝鐐硅壊鍧楃洿鎺ヨ皟鑹叉澘鍙栬壊
- **闅愯棌鎷奸煶鏃惰鍓┖鐧?*锛歚trim_hidden_preedit_gap`锛岃В鍐崇紪鐮佸尯闅愯棌鍚庡€欓€夌獥涓婃柟鐣欎竴鏉＄┖鐧界殑闂

### 瀛椾綋锛堝啓鍏?`config.toml`锛?
- **鍊欓€夌獥涓诲瓧浣?* `english_font`锛堟按鏉夎缃噷鍙€屽€欓€夌獥涓诲瓧浣撱€嶏紝榛樿 `Segoe UI`锛?- **涓枃鍥為€€瀛椾綋** `font`
- **瀛楀彿** `font_size`
- 瀛椾綋涓嬫媺鑷姩鍒楀嚭绯荤粺宸茶瀛椾綋锛岄瑙堝疄鏃剁敤鎵€閫夊瓧浣撴覆鏌?
### 缂栫爜锛堟嫾闊筹級鏄剧ず

- **鍊欓€夌獥鏄剧ず鎷奸煶** `candidate_window_preedit_style`锛坄pinyin` / `empty`锛?- **杈撳叆妗嗘樉绀烘嫾闊?* `tsf_preedit_style`锛坄raw` / `empty`锛?
### 鍏跺畠

- **瀹炴椂棰勮**锛? 涓€欓€夐」鐨勬ā鎷熷€欓€夌獥锛屾敼浠讳綍鍙傛暟绔嬪埢閲嶇粯
- **娣辫壊 / 娴呰壊涓婚**锛氬伐鍏疯嚜韬袱濂楅厤鑹诧紝鍙垏鎹?- **鍗＄墖鎶樺彔**锛氭爣棰樻爮鐐瑰嚮鎶樺彔/灞曞紑
- **璁颁綇涓婃鐘舵€?*锛氫富棰樸€佹姌鍙犵姸鎬併€佹墍閫夌毊鑲ゃ€佺獥鍙ｅぇ灏?- **涓€閿噸鍚?*锛歚淇濆瓨骞堕噸鍚痐 浼氭彁鏉冪粨鏉?`MetasequoiaImeServer` 骞堕噸鏂版媺璧凤紝鏀瑰畬绔嬪埢鐢熸晥

![娴呰壊涓婚](screenshots/light.png)

## 蹇€熷紑濮?
1. 涓嬭浇 [Releases](../../releases) 閲岀殑 `MetasequoiaTuner.exe`
2. 鍙屽嚮杩愯锛堝崟鏂囦欢锛屼緷璧栫郴缁熻嚜甯︾殑 .NET Framework 4.8锛屾棤闇€瀹夎锛?3. 宸︿晶閫夌毊鑲?鈫?鍙宠竟璋冨弬鏁?鈫?鐐广€屼繚瀛樺苟閲嶅惎銆?
> 銆岄噸鍚按鏉夈€嶄細寮?UAC锛歚MetasequoiaImeServer` 浠ョ鐞嗗憳鏉冮檺杩愯锛屼笉鎻愭潈鏉€涓嶆帀銆?
## 鑷繁缂栬瘧

闇€瑕佺郴缁熻嚜甯︾殑 .NET Framework 4.x锛圵indows 10/11 榛樿鑷甫锛夛細

```powershell
.\build.ps1
# 浜х墿锛歞ist\MetasequoiaTuner.exe
```

娌℃湁浠讳綍绗笁鏂逛緷璧栵紝`csc.exe` 鐩存帴缂栬瘧锛屼笉闇€瑕佽 Visual Studio銆?
## 閰嶇疆鏂囦欢浣嶇疆

| 鍐呭 | 璺緞 |
|---|---|
| 鐨偆 | `%LOCALAPPDATA%\metasequoiaime\skins\<鐨偆鍚?\skin.toml` |
| 鍏ㄥ眬澶栬锛堝瓧浣撱€佺紪鐮侊級 | `%LOCALAPPDATA%\metasequoiaime\config.toml` |
| 鏈伐鍏疯嚜韬缃?| `%LOCALAPPDATA%\Programs\MetasequoiaTuner\settings.ini` |

## 璇存槑

- 鏈伐鍏峰彧鏄?*鏀归厤缃枃浠?+ 閲嶅惎杩涚▼**鐨勫浘褰㈢晫闈紝涓嶄慨鏀规按鏉夋湰浣擄紝涔熶笉娉ㄥ叆浠讳綍涓滆タ
- 淇濆瓨鍓嶅缓璁浠?`metasequoiaime` 鐩綍锛涙敼鍧忎簡鎶婂搴旈敭鍒犳帀鍗冲彲鎭㈠
- 銆屼繚瀛樺苟閲嶅惎銆嶄細鍚屾椂鍐欏叆 `config.toml`锛屽鏋滄按鏉夎缃獥鍙ｅ紑鐫€锛屽缓璁厛鍏虫帀锛岄伩鍏嶄簰鐩歌鐩?
## 鑷磋阿

- [姘存潐杈撳叆娉?Metasequoia IME](https://github.com/metasequoiaime) 鈥斺€?鐨偆涓庨厤缃牸寮忕殑鐏垫劅鏉ユ簮
- 鐨偆瀛楁鍚箟鍙傝€?`msime-windows` 婧愮爜涓?`candidate_skin_catalog.cpp` / `CandidateList.cpp`

## License

[MIT](LICENSE)

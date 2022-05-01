using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float seconds = 0.4f;
    public AudioSource bgSound, textSound, fxSound;
    public AudioClip clip;
    public Image bg, creditsBg;
    public Text text, npcName, dialogs, allScript, locname;
    public Animation anim, credits;
    public string[] lines;
    public int currentLine;
    public bool canGo = true, typingText, paused;
    bool firstBgLoad = true;
    public Animation blackAnim, creditsBlackAnim;
    public GameObject button1, button2;
    public GameObject[] saves, menuLoads;
    public Text saveLoadText, youReallyWantTo, modsPath, modsText, savesPathText;
    public string path, gameName = "gamescript";
    bool mainStory;
    public InputField modNameInput;

    public RectTransform textSpeed, bgSoundVolume, textSoundVolume, fxSoundVolume;

    public int button1Index, button2Index;

    public void StartGame()
    {
        currentLine = 0;
        firstBgLoad = true;
        blackAnim.GetComponent<Image>().color = new Color(0, 0, 0, 1);
        text.text = "";
        npcName.text = "";
        locname.text = "";

        if (gameName == "gamescript")
        {
            lines = Resources.Load<TextAsset>("gamescript").text.Split('\n', '\r');
            mainStory = true;
        }
        else lines = File.ReadAllLines(Application.persistentDataPath + "/mods/" + gameName);
        Debug.Log(Application.persistentDataPath);
        NextLine();
    }

    private void Start()
    {
        modsPath.text =  "<i><b>" + Application.persistentDataPath + "/mods" + "</b></i>";
        if (PlayerPrefs.GetInt("firstLaunch") == 0)
        {
            PlayerPrefs.SetInt("firstLaunch", 1);
            PlayerPrefs.SetFloat("bgSound", bgSound.volume);
            PlayerPrefs.SetFloat("textSound", textSound.volume);
            PlayerPrefs.SetFloat("fxSound", fxSound.volume);
            PlayerPrefs.SetFloat("textSpeed", seconds * 10);
        }
        else
        {
            bgSound.volume = PlayerPrefs.GetFloat("bgSound");
            textSound.volume = PlayerPrefs.GetFloat("textSound");
            fxSound.volume = PlayerPrefs.GetFloat("fxSound");
            seconds = PlayerPrefs.GetFloat("textSpeed") / 10;

            bgSoundVolume.sizeDelta = new Vector2(bgSound.volume * 474.3842f, bgSoundVolume.sizeDelta.y);
            textSoundVolume.sizeDelta = new Vector2(textSound.volume * 474.3842f, textSoundVolume.sizeDelta.y);
            fxSoundVolume.sizeDelta = new Vector2(fxSound.volume * 474.3842f, fxSoundVolume.sizeDelta.y);
            textSpeed.sizeDelta = new Vector2(seconds * 10 * 474.3842f, textSpeed.sizeDelta.y);
        }
    }

    public void NextLine()
    {
        if (lines.Length < currentLine) return;

        if (!canGo)
        {
            if (typingText)
            {
                string line1 = lines[currentLine - 1];
                int index = line1.IndexOf(':');
                StopAllCoroutines();
                text.text = "- " + line1.Substring(index + 2);
                canGo = true;
                typingText = false;
            }
            return;

        }

        string line = lines[currentLine];
        canGo = false;
        Debug.Log(line);
        currentLine += 1;

        if (line.Split(' ')[0] == "/bg")
        {
            string command = line.Replace("/bg ", "");
            StartCoroutine(switchBg(command));
            allScript.text += "\n*changed background to " + command + "*";
        }

        else if (line.Split(' ')[0] == "/bgsound")
        {
            string command = line.Replace("/bgsound ", "");
            StartCoroutine(switchBgSound(command));
            allScript.text += "\n*changed soundtrack to " + command + "*";
            canGo = true;
            NextLine();
        }

        else if (line.Split(' ')[0] == "/locname")
        {
            locname.text = line.Replace("/locname ", "");
            allScript.text += "\n*changed location name to " + locname.text + "*";
            canGo = true;
            NextLine();
        }

        else if (line.Split(' ')[0] == "/changescript")
        {
            string command = line.Replace("/changescript ", "");
            gameName = command;
            if (mainStory) lines = Resources.Load<TextAsset>(gameName).text.Split('\n', '\r');
            else lines = File.ReadAllLines(Application.persistentDataPath + "/mods/" + gameName);
            currentLine = 0;
            canGo = true;
            NextLine();
        }

        else if (line.Split(' ')[0] == "/wait")
        {
            StartCoroutine(Wait(Single.Parse(line.Split(' ')[1])));
        }

        else if (line.Split(' ')[0] == "/fxsound")
        {
            fxSound.Stop();
            fxSound.clip = Resources.Load<AudioClip>(line.Replace("/fxsound ", ""));
            fxSound.Play();
        }

        else if (line.Split(' ')[0] == "/button1")
        {
            button1.GetComponentInChildren<Text>().text = line.Replace("/button1 ", "");
            button1.SetActive(true);
            button1Index = currentLine;
            int a = 0;
            for (int i = currentLine; true; i++)
            {
                if (lines[i] == "}")
                {
                    a = i;
                    break;
                }
            }

            for (int i = a; true; i++)
            {
                if (lines.Length <= i) return;
                Debug.Log(lines[i]);
                if (lines[i].Split(' ')[0] == "/button2")
                {
                    currentLine = i;
                    canGo = true;
                    NextLine();
                    return;
                }

                else if (lines[i].Length > 1)
                {
                    currentLine = i;
                    return;
                }
            }
        }

        else if (line == "}")
        {
            for (int i = currentLine; true; i++)
            {
                if (lines.Length <= i) return;
                Debug.Log(lines[i]);
                if (lines[i].Split(' ')[0] == "/button2")
                {
                    for (int a = i; true; a++)
                    {
                        if (lines.Length <= a) return;

                        if (lines[a] == "}")
                        {
                            currentLine = a + 1;
                            break;
                        }
                    }
                    canGo = true;
                    NextLine();
                    return;
                }

                else if (lines[i].Length > 1)
                {
                    currentLine = i;
                    canGo = true;
                    NextLine();
                    return;
                }
            }
        }

        else if (line.Split(' ')[0] == "/button2")
        {
            button2.GetComponentInChildren<Text>().text = line.Replace("/button2 ", "");
            button2.SetActive(true);
            button2Index = currentLine - 1;

            int a = 0;
            for (int i = currentLine; true; i++)
            {
                if (lines[i] == "}")
                {
                    a = i + 1;
                    break;
                }
            }

            for (int i = a; true; i++)
            {
                if (lines.Length <= i)
                {
                    currentLine = i;
                    return;
                }
                Debug.Log(i);
                if (lines[i].Length > 1)
                {
                    currentLine = i;
                    return;
                }
            }
        }

        else if (line.Length > 1)
        {
            int index = line.IndexOf(':');
            npcName.text = line.Substring(0, index);
            allScript.text += "\n" + npcName.text + " : " + line.Substring(index + 2);
            if (npcName.text == "_storyteller_") npcName.text = "";
            StartCoroutine(TextGeneration("- " + line.Substring(index + 2)));
            string nickname = npcName.text + " : ";
            if (npcName.text == "") nickname = "   ";
            dialogs.text += "\n" + nickname + line.Substring(index + 2);
        }

        else
        {
            canGo = true;
            NextLine();
        }
    }

    IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        canGo = true;
        NextLine();
    }

    IEnumerator TextGeneration(string _text)
    {
        typingText = true;
        char[] letters = _text.ToCharArray();
        textSound.clip = clip;
        int a = 1;
        text.text = "";
        foreach (char c in letters)
        {
            if (typingText == false)
            {
                canGo = true;
                yield break;
            }
            text.text += c;
            if (a % 2 == 0)
            {
                textSound.Stop();
                textSound.Play();
            }
            a++;
            yield return new WaitForSeconds(seconds);
        }
        canGo = true;
        typingText = false;
    }

    public void GoToMenu()
    {
        Time.timeScale = 1;
        typingText = false;
        StartCoroutine(switchBgSound("menusound"));
        
    }

    IEnumerator switchBg(string bgname, bool notContinue = false)
    {
        if (!firstBgLoad)
        {
            blackAnim.Play("show");
            while (blackAnim.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            firstBgLoad = false;
        }

        bg.sprite = Resources.Load<Sprite>(bgname);
        canGo = true;
        blackAnim.Play("hide");
        yield return new WaitForSeconds(0.15f);
        if (!notContinue) NextLine();
    }

    IEnumerator switchBgSound(string bgsoundname)
    {
        float volume = bgSound.volume;
        float value = volume / 10;
        while (bgSound.volume > 0)
        {
            bgSound.volume -= value;
            yield return new WaitForEndOfFrame();
        }
        bgSound.clip = Resources.Load<AudioClip>(bgsoundname);
        bgSound.Play();
        while (bgSound.volume < volume)
        {
            bgSound.volume += value;
            yield return new WaitForEndOfFrame();
        }

        bgSound.volume = volume;
    }

    public void Pause(float value)
    {
        Time.timeScale = value;
    }

    int load;
    public bool saveLoad;

    public void SetLoad(int _load)
    {
        load = _load;
    }

    public void SaveLoad(bool _saveLoad)
    {
        saveLoad = _saveLoad;

        if (saveLoad)
        {
            youReallyWantTo.text = "save current game state?";
            saveLoadText.text = "save game";
        }
        else
        {
            youReallyWantTo.text = "load saved game state?";
            saveLoadText.text = "load game";
        }
    }

    public void YesIWantTo()
    {
        if (saveLoad)
        {
            string line = lines[currentLine-1];
            int index = line.IndexOf(':');
            string npcName = line.Substring(0, index) + ": ";
            if (npcName == "_storyteller_: ") 
            {
                npcName = "";
            }
            int len = 30;
            string dots = "...";
            if (line.Substring(index + 2).Length < 30)
            {
                len = line.Substring(index + 2).Length;
                dots = "";
            }
            string currentText = (npcName + line.Substring(index + 2)).Substring(0, len).Trim('.') + "...";
            int mainstory = 0;
            if (mainStory) mainstory = 1;
            PlayerPrefs.SetString("load" + load, currentLine-1 + "℁" + bg.sprite.name + "℁" + bgSound.clip.name + "℁" + locname.text + "℁" + currentText + "℁" + gameName + "℁" + mainstory);
        }
        else
        {
            string[] save = PlayerPrefs.GetString("load" + load).Split('℁');
            int saveindex;
            if (Int32.TryParse(save[0], out saveindex))
            {
                if (save[6] == "1") lines = Resources.Load<TextAsset>(save[5]).text.Split('\n', '\r');
                else lines = File.ReadAllLines(Application.persistentDataPath + "/mods/" + save[5]);

                if (save[6] == "1") mainStory = true;
                else mainStory = false;

                currentLine = saveindex;
                bgSound.clip = Resources.Load<AudioClip>(save[2]);
                bgSound.Play();
                locname.text = save[3];
                firstBgLoad = true;
                blackAnim.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                StopAllCoroutines();
                StartCoroutine(switchBg(save[1]));
            }
        }
    }

    public void StartModGame()
    {
        if (File.Exists(Application.persistentDataPath + "/mods/" + modNameInput.text))
        {
            gameName = modNameInput.text;
            mainStory = false;
            StartGame();
        }
        modNameInput.text = "";
    }

    public void ShowSavesLoads()
    {
        for (int i = 0; i < saves.Length; i++)
        {
            string _index = PlayerPrefs.GetString("load" + i).Split('℁')[0];
            int saveindex;
            if (Int32.TryParse(_index, out saveindex))
            {
                saves[i].GetComponentInChildren<Text>().text = menuLoads[i].GetComponentInChildren<Text>().text = PlayerPrefs.GetString("load" + i).Split('℁')[4];
            }
            else
            {
                saves[i].GetComponentInChildren<Text>().text = menuLoads[i].GetComponentInChildren<Text>().text = "empty slot";
            }
        }
    }

    public void ShowMods()
    {
        modsText.text = "";
        modsText.transform.parent.GetComponent<RectTransform>().sizeDelta -= new Vector2(0, modsText.transform.parent.GetComponent<RectTransform>().sizeDelta.y);
        string[] mods = Directory.GetFiles(Application.persistentDataPath + "/mods", "*.gcs");
        foreach (string mod in mods)
        {
            modsText.text += mod.Split('\\')[mod.Split('\\').Length - 1] + "\n";
            modsText.transform.parent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 38);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public IEnumerator CreditsBg()
    {
        while (true)
        {
            while (credits.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            creditsBlackAnim.Play("show");
            while (creditsBlackAnim.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            Sprite[] spr = Resources.LoadAll<Sprite>("");
            bg.sprite = spr[UnityEngine.Random.Range(0, spr.Length)];
            creditsBlackAnim.Play("hide");
            yield return new WaitForSeconds(10);
        }
    }

    public void ChangeSomething(string _value)
    {
        string name = _value.Split('=')[0];
        Debug.Log(_value.Split('=')[1]);
        float value = Single.Parse(_value.Split('=')[1]);

        PlayerPrefs.SetFloat(name, PlayerPrefs.GetFloat(name) + value);
        bgSound.volume = PlayerPrefs.GetFloat("bgSound");
        textSound.volume = PlayerPrefs.GetFloat("textSound");
        fxSound.volume = PlayerPrefs.GetFloat("fxSound");
        seconds = PlayerPrefs.GetFloat("textSpeed") / 10;

        bgSoundVolume.sizeDelta = new Vector2(bgSound.volume * 474.3842f, bgSoundVolume.sizeDelta.y);
        textSoundVolume.sizeDelta = new Vector2(textSound.volume * 474.3842f, textSoundVolume.sizeDelta.y);
        fxSoundVolume.sizeDelta = new Vector2(fxSound.volume * 474.3842f, fxSoundVolume.sizeDelta.y);
        textSpeed.sizeDelta = new Vector2(seconds * 10 * 474.3842f, textSpeed.sizeDelta.y);
    }

    public void DeleteSaves()
    {
        PlayerPrefs.DeleteAll();
    }

    public void SaveSaves()
    {
        string text = "";
        for (int i = 0; i < saves.Length; i++)
        {
            string[] save = PlayerPrefs.GetString("load" + i).Split('℁');
            if (Int32.TryParse(save[0], out _))
            {
                text += PlayerPrefs.GetString("load" + i);
            }
            else
            {
                text += "nothing";
            }
            text += "\n\n";
        }

        if (!File.Exists(Application.persistentDataPath + "/saves")) Directory.CreateDirectory(Application.persistentDataPath + "/saves");

        string name = "save_" + DateTime.Now.ToString("dd.MM.yyyy");
        if (File.Exists(Application.persistentDataPath + "/saves/" + name + ".gcb"))
        {
            int a = 1;
            while (File.Exists(Application.persistentDataPath + "/saves/" + name + "(" + a + ")" + ".gcb")) a += 1;
            name += "(" + a + ")";
        }

        using (var sw = File.CreateText(Application.persistentDataPath + "/saves/" + name + ".gcb"))
        {
            sw.Write(text);
        }

        savesPathText.text = "<i><b>" + Application.persistentDataPath + "/saves/" + name + ".gcb</b></i>";
    }

    public void Button1()
    {
        currentLine = button1Index + 1;
        canGo = true;
        NextLine();
        button1.SetActive(false);
        button2.SetActive(false);
    }

    public void Button2()
    {
        currentLine = button2Index + 1;
        canGo = true;
        NextLine();
        button1.SetActive(false);
        button2.SetActive(false);
    }
}

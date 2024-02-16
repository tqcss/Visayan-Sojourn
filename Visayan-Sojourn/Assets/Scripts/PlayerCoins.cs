using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCoins : MonoBehaviour
{
    public float globalCoins;
    public float initialCoins;
    public float coinsGenerated;
    public float coinGeneratorMax;
    public float coinsPerGenerate;
    public float generateMaxCooldown;
    public float generateCooldown;
    public bool inCooldown;

    private UpdateDisplayMain _updateDisplayMain;
    private static GameObject s_instance {set; get;}

    private void Awake()
    {
        // Will not destroy the script when on the next loaded scene
        if (s_instance != null) 
            Destroy(s_instance);
        s_instance = gameObject;
        DontDestroyOnLoad(s_instance);

        // Reference the scripts from game objects
        _updateDisplayMain = GameObject.FindGameObjectWithTag("mainScript").GetComponent<UpdateDisplayMain>();

        // Set initial values to the variables
        coinsGenerated = PlayerPrefs.GetFloat("CoinsGenerated", 0);
        generateCooldown = PlayerPrefs.GetFloat("GenerateCooldown", generateMaxCooldown);
    }

    private void Start()
    {
        OfflineCooldown(PlayerPrefs.GetInt("CheckOfflineCoinBag", 1));
        _updateDisplayMain.UpdateDisplayCoins();
    }
    
    private void OfflineCooldown(int offline)
    {
        if (offline == 1)
        {
            PlayerPrefs.SetInt("CheckOfflineCoinBag", 0);
            // Get the current time
            DateTime timeCurrent = DateTime.Now;
            if (PlayerPrefs.HasKey("SavedTime"))
            {
                // Get the time saved after the user quitted from the previous session
                DateTime timeSaved = DateTime.Parse(PlayerPrefs.GetString("SavedTime"));
                // Compute the amount of time the user is offline
                TimeSpan timePassed = timeCurrent - timeSaved;
                float timeLeftFromOffline = (float)timePassed.TotalSeconds;

                // Decrease generate cooldown or increase coins generated
                // based on the amount of time the user is offline
                while (timeLeftFromOffline > 0)
                {
                    float coinsGeneratedCurrent = PlayerPrefs.GetFloat("CoinsGenerated", 0);
                    float generateCooldownCurrent = PlayerPrefs.GetFloat("GenerateCooldown", generateMaxCooldown);

                    // Set time left from offline to 0 if the coins generated at maximum 
                    if (coinsGeneratedCurrent > coinGeneratorMax)
                        timeLeftFromOffline = 0;
                    
                    // Decrease the time left from offline by the current generate cooldown
                    // and increment coins generated by coins per generate if there are more time left from offline than generate cooldown
                    if (timeLeftFromOffline > generateCooldownCurrent)
                    {
                        timeLeftFromOffline -= generateCooldownCurrent;
                        PlayerPrefs.SetFloat("CoinsGenerated", coinsGeneratedCurrent + coinsPerGenerate);
                        PlayerPrefs.SetFloat("GenerateCooldown", generateMaxCooldown);
                        generateCooldown = 0;
                        _updateDisplayMain.UpdateDisplayCoinBag();
                    }
                    // Decrease the current generate cooldown by the time left from offline
                    // if there are more generate cooldown than time left from offline
                    else
                    {
                        PlayerPrefs.SetFloat("GenerateCooldown", generateCooldownCurrent - timeLeftFromOffline);
                        generateCooldown = generateCooldownCurrent - timeLeftFromOffline;
                        timeLeftFromOffline = 0;
                    }
                }
            }
        }
    }
    
    private void Update()
    {
        // Automatically update the player global coins
        globalCoins = PlayerPrefs.GetFloat("GlobalCoins", initialCoins);
        _updateDisplayMain.UpdateDisplayCoins();

        coinsGenerated = PlayerPrefs.GetFloat("CoinsGenerated", 0);
        _updateDisplayMain.UpdateDisplayCoinBag();

        // Check if the coin generated is less than the maximum
        if ((int)coinsGenerated < coinGeneratorMax)
        {
            if (!inCooldown)
            {
                // Activate the cooldown if it is inactive
                inCooldown = true;
            }
            else
            {
                // Decrease the generate cooldown if it is more than 0
                if (generateCooldown > 0)
                {
                    generateCooldown -= Time.deltaTime;
                }
                // Increment coins generated by coins per generate and reset the generate cooldown if it reaches 0
                else if (generateCooldown <= 0)
                {
                    PlayerPrefs.SetFloat("CoinsGenerated", coinsGenerated + coinsPerGenerate);
                    PlayerPrefs.SetFloat("GenerateCooldown", generateMaxCooldown);
                    generateCooldown = generateMaxCooldown;
                    inCooldown = false;
                }

                // Set the generate cooldown to its floor
                if (Mathf.FloorToInt(generateCooldown % 1) == 0)
                {
                    PlayerPrefs.SetFloat("GenerateCooldown", Mathf.FloorToInt(generateCooldown));
                }
            }
        }
        else
        {
            PlayerPrefs.SetFloat("GenerateCooldown", generateMaxCooldown);
            generateCooldown = generateMaxCooldown;
            inCooldown = false;
        }
    }

    public void CollectCoinBag()
    {
        IncreaseCoins(PlayerPrefs.GetFloat("CoinsGenerated", 0));
        PlayerPrefs.SetFloat("CoinsGenerated", 0);
    }

    public void IncreaseCoins(float increase)
    {
        // Increase coins by the amount prompted
        float globalCoins = PlayerPrefs.GetFloat("GlobalCoins", initialCoins);
        PlayerPrefs.SetFloat("GlobalCoins", globalCoins + increase);
    }

    public void DecreaseCoins(float decrease)
    {
        // Decrease coins by the amount prompted
        float globalCoins = PlayerPrefs.GetFloat("GlobalCoins", initialCoins);
        PlayerPrefs.SetFloat("GlobalCoins", (globalCoins >= decrease) ? globalCoins - decrease : 0);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("CheckOfflineCoinBag", 1);
        PlayerPrefs.Save();
    }
}

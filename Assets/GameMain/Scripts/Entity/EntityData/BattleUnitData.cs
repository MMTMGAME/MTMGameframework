using System.Collections;
using System.Collections.Generic;
using GameFramework.DataTable;
using GameMain;
using Unity.VisualScripting;
using UnityEngine;




[System.Serializable]
[IncludeInSettings(true)]
public class BattleUnitData : EntityData
{
    
    
    [SerializeField]
    private List<WeaponData> m_WeaponDatas = new List<WeaponData>();

    [SerializeField]
    private List<ArmorData> m_ArmorDatas = new List<ArmorData>();

    [SerializeField]
    private int m_MaxHP = 0;

    [SerializeField]
    private int m_Defense = 0;

    [SerializeField]
    private int m_DeadEffectId = 0;

    [SerializeField]
    private int m_DeadSoundId = 0;

    private string weaponPathKey;
   

    public int DieScore;

    public int baseHP;
    public int baseDefense;
    public int baseAttack;
    public int baseMP;
    public int baseMoveSpeed;
    public int baseActionSpeed;

    public string[] skills;
    public string[] buffs;
    
    public string[] tags;

    [Header("死亡后过久隐藏(ms)")]
    public long hideTime;

    public CampType Camp
    {
        get;
    }
    public string GetWeaponPath(int index)
    {
        switch (index)
        {
            case 0:
                return weaponPathKey;
            
        }

        return weaponPathKey;
    }

    public BattleUnitData(int entityId, int typeId, CampType camp) : base(entityId, typeId)
    {

        this.Camp = camp;
        
        //IDataTable<DRBattleUnit> dtBattleUnits = GameEntry.DataTable.GetDataTable<DRBattleUnit>();
        var drBattleUnit = GameEntry.SoDataTableComponent.GetSoDataRow<BattleUnitDataRow>(TypeId);
        if (drBattleUnit == null)
        {
            return;
        }

        weaponPathKey = drBattleUnit.weaponPathKey;
        

        DieScore = drBattleUnit.dieScore;

        baseHP = drBattleUnit.baseHp;
        baseAttack = drBattleUnit.baseAttack;
        baseDefense = drBattleUnit.baseDefense;
        baseMP = drBattleUnit.baseMp;
        baseMoveSpeed = drBattleUnit.baseMoveSpeed;
        baseActionSpeed = drBattleUnit.baseActionSpeed;

        this.tags = drBattleUnit.tags;
        
        for (int index = 0, weaponId = 0; (weaponId = drBattleUnit.GetWeaponIdAt(index)) > 0; index++)
        {
            AttachWeaponData(new WeaponData(GameEntry.Entity.GenerateSerialId(), weaponId, Id, Camp,index));
        }

        for (int index = 0, armorId = 0; (armorId = drBattleUnit.GetArmorIdAt(index)) > 0; index++)
        {
            AttachArmorData(new ArmorData(GameEntry.Entity.GenerateSerialId(), armorId, Id, Camp));
        }

        m_DeadEffectId = drBattleUnit.deadEffectId;
        m_DeadSoundId = drBattleUnit.deadSoundId;

        this.hideTime = drBattleUnit.hideTime;
        this.skills = drBattleUnit.skills;
        this.buffs = drBattleUnit.buffs;
    }


    public int DeadEffectId
    {
        get
        {
            return m_DeadEffectId;
        }
    }

    public int DeadSoundId
    {
        get
        {
            return m_DeadSoundId;
        }
    }
    
    public List<WeaponData> GetAllWeaponDatas()
    {
        return m_WeaponDatas;
    }

  
    public void AttachWeaponData(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            return;
        }

        if (m_WeaponDatas.Contains(weaponData))
        {
            return;
        }

        m_WeaponDatas.Add(weaponData);
    }

    public void DetachWeaponData(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            return;
        }

        m_WeaponDatas.Remove(weaponData);
    }

    public List<ArmorData> GetAllArmorDatas()
    {
        return m_ArmorDatas;
    }

    public void AttachArmorData(ArmorData armorData)
    {
        if (armorData == null)
        {
            return;
        }

        if (m_ArmorDatas.Contains(armorData))
        {
            return;
        }

        m_ArmorDatas.Add(armorData);
       
    }

    public void DetachArmorData(ArmorData armorData)
    {
        if (armorData == null)
        {
            return;
        }

        m_ArmorDatas.Remove(armorData);
        
    }

    
}

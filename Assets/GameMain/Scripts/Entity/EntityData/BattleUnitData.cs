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

    private string weaponPath0;
    private string weaponPath1;
    private string weaponPath2;

    public int DieScore;

    public int baseHP;
    public int baseDefense;
    public int baseAttack;
    public int baseMP;
    public int baseMoveSpeed;
    public int baseActionSpeed;

    public CampType Camp
    {
        get;
    }
    public string GetWeaponPath(int index)
    {
        switch (index)
        {
            case 0:
                return weaponPath0;
            case 1:
                return weaponPath1;
            case 2:
                return weaponPath2;
        }

        return "";
    }

    public BattleUnitData(int entityId, int typeId, CampType camp) : base(entityId, typeId)
    {

        this.Camp = camp;
        
        IDataTable<DRBattleUnit> dtBattleUnits = GameEntry.DataTable.GetDataTable<DRBattleUnit>();
        DRBattleUnit drBattleUnit = dtBattleUnits.GetDataRow(TypeId);
        if (drBattleUnit == null)
        {
            return;
        }

        weaponPath0 = drBattleUnit.WeaponPath0;
        weaponPath1 = drBattleUnit.WeaponPath1;
        weaponPath2 = drBattleUnit.WeaponPath2;

        DieScore = drBattleUnit.DieScore;

        baseHP = drBattleUnit.BaseHP;
        baseAttack = drBattleUnit.BaseAttack;
        baseDefense = drBattleUnit.BaseAttack;
        baseMP = drBattleUnit.BaseMP;
        baseMoveSpeed = drBattleUnit.BaseMoveSpeed;
        baseActionSpeed = drBattleUnit.BaseActionSpeed;
        
        for (int index = 0, weaponId = 0; (weaponId = drBattleUnit.GetWeaponIdAt(index)) > 0; index++)
        {
            AttachWeaponData(new WeaponData(GameEntry.Entity.GenerateSerialId(), weaponId, Id, Camp,index));
        }

        for (int index = 0, armorId = 0; (armorId = drBattleUnit.GetArmorIdAt(index)) > 0; index++)
        {
            AttachArmorData(new ArmorData(GameEntry.Entity.GenerateSerialId(), armorId, Id, Camp));
        }

        m_DeadEffectId = drBattleUnit.DeadEffectId;
        m_DeadSoundId = drBattleUnit.DeadSoundId;

        
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

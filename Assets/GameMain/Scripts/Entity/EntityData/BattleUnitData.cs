using System.Collections;
using System.Collections.Generic;
using GameFramework.DataTable;
using GameMain;
using UnityEngine;

[System.Serializable]
public class BattleUnitData : TargetableObjectData
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

    public BattleUnitData(int entityId, int typeId, CampType camp) : base(entityId, typeId, camp)
    {
        IDataTable<DRBattleUnit> dtBattleUnits = GameEntry.DataTable.GetDataTable<DRBattleUnit>();
        DRBattleUnit drBattleUnit = dtBattleUnits.GetDataRow(TypeId);
        if (drBattleUnit == null)
        {
            return;
        }

        
        for (int index = 0, weaponId = 0; (weaponId = drBattleUnit.GetWeaponIdAt(index)) > 0; index++)
        {
            AttachWeaponData(new WeaponData(GameEntry.Entity.GenerateSerialId(), weaponId, Id, Camp));
        }

        for (int index = 0, armorId = 0; (armorId = drBattleUnit.GetArmorIdAt(index)) > 0; index++)
        {
            AttachArmorData(new ArmorData(GameEntry.Entity.GenerateSerialId(), armorId, Id, Camp));
        }

        m_DeadEffectId = drBattleUnit.DeadEffectId;
        m_DeadSoundId = drBattleUnit.DeadSoundId;

        HP = m_MaxHP;
    }

    /// <summary>
    /// 最大生命。
    /// </summary>
    public override int MaxHP
    {
        get
        {
            return m_MaxHP;
        }
    }

    /// <summary>
    /// 防御。
    /// </summary>
    public int Defense
    {
        get
        {
            return m_Defense;
        }
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
        RefreshData();
    }

    public void DetachArmorData(ArmorData armorData)
    {
        if (armorData == null)
        {
            return;
        }

        m_ArmorDatas.Remove(armorData);
        RefreshData();
    }

    private void RefreshData()
    {
        m_MaxHP = 0;
        m_Defense = 0;
        for (int i = 0; i < m_ArmorDatas.Count; i++)
        {
            m_MaxHP += m_ArmorDatas[i].MaxHP;
            m_Defense += m_ArmorDatas[i].Defense;
        }

        if (HP > m_MaxHP)
        {
            HP = m_MaxHP;
        }
    }
}

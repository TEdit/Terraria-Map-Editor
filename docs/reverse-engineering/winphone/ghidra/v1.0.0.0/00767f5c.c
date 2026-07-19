
void FUN_00767f5c(void)

{
  byte bVar1;
  int *piVar2;
  int iVar3;
  undefined4 uVar4;
  undefined4 uVar5;
  int iVar6;
  undefined2 *puVar7;
  longlong lVar8;
  uint local_13c [4];
  undefined4 local_12c;
  uint local_128;
  uint local_124 [4];
  undefined4 local_114;
  uint local_110;
  uint local_10c [4];
  undefined4 local_fc;
  uint local_f8;
  uint local_f4 [4];
  undefined4 local_e4;
  uint local_e0;
  uint local_dc [4];
  undefined4 local_cc;
  uint local_c8;
  uint local_c4 [4];
  undefined4 local_b4;
  uint local_b0;
  uint local_ac [4];
  undefined4 local_9c;
  uint local_98;
  uint local_94 [4];
  undefined4 local_84;
  uint local_80;
  uint local_7c [4];
  undefined4 local_6c;
  uint local_68;
  undefined4 local_64;
  undefined4 uStack_60;
  undefined4 local_5c [5];
  uint local_48;
  byte local_44;
  char local_43;
  short local_42;
  char local_40 [4];
  undefined1 auStack_3c [4];
  undefined4 local_38;
  undefined4 local_34 [4];
  
  lVar8 = FUN_007ffb80();
  uVar5 = (undefined4)((ulonglong)lVar8 >> 0x20);
  piVar2 = (int *)lVar8;
  local_64 = 0xfffffffe;
  uStack_60 = 0xfffffffe;
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_34,4);
  local_38 = 0;
  if (0x2fffffffff < lVar8) {
    (**(code **)(*piVar2 + 0x1c))(piVar2,&local_38,4);
  }
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_00902e98,4);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_42,2);
  DAT_00902e9c = (int)local_42;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_00902ea4,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_00902ea0,2);
  iVar3 = (int)DAT_00902ea4;
  DAT_00902e9c = iVar3 << 4;
  DAT_00902e98 = (int)DAT_00902ea0 << 4;
  iVar6 = (int)((ulonglong)((longlong)(int)DAT_00902ea0 * (longlong)DAT_00768794) >> 0x20);
  DAT_010502fc = (iVar6 >> 3) - (iVar6 >> 0x1f);
  iVar3 = (int)((ulonglong)((longlong)iVar3 * (longlong)DAT_00768790) >> 0x20) + iVar3;
  DAT_01050300 = (iVar3 >> 3) - (iVar3 >> 0x1f);
  FUN_00762228();
  DAT_01033974 = local_34[0];
  DAT_01033970 = local_38;
  uVar4 = FUN_0058ef48();
  FUN_007369b0(uVar4,4,&DAT_00fd9438);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_010338cc,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_010338c8,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_42,2);
  DAT_00902ebc = (int)local_42;
  DAT_01050304 = DAT_00902ebc << 4;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_42,2);
  DAT_01033960 = (int)local_42;
  DAT_0103395c = DAT_01033960 << 4;
  iVar3 = (int)((ulonglong)
                ((longlong)((DAT_00902ea4 - DAT_00902ebc) + -0xe6) * (longlong)DAT_0076878c) >> 0x20
               );
  DAT_01033958 = DAT_00902ebc + (iVar3 - (iVar3 >> 0x1f)) * 6 + -5;
  DAT_010338d4 = DAT_01033958 * 0x10;
  FUN_00735318(&DAT_01030850,piVar2,uVar5);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_01033968,2);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_01033964,2);
  FUN_00749d20(piVar2,uVar5);
  FUN_00659a38(piVar2,uVar5);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a76f,1);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a780,1);
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_44,1);
  DAT_0102a778 = (uint)local_44;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a77c,4);
  if (DAT_0102a77c == 0) {
    DAT_00902c38 = 0xffff;
    DAT_00902c30 = 0xffff;
    DAT_00902c34 = 0xffff;
  }
  else {
    DAT_00902c30 = 0x6b;
    DAT_00902c34 = 0x6c;
    DAT_00902c38 = 0x6f;
  }
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_010338d3,1);
  if (0x37ffffffff < lVar8) {
    (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_01030860,2);
  }
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_44,1);
  DAT_010339ac = (uint)local_44;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_42,2);
  DAT_010339a8 = (int)local_42;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&local_44,1);
  DAT_010339a0 = (uint)local_44;
  (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_010339a4,4);
  if (0x39ffffffff < lVar8) {
    iVar3 = 0;
    do {
      (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a710 + iVar3,1);
      iVar3 = iVar3 + 1;
    } while (iVar3 < 4);
    puVar7 = &DAT_0102a714;
    iVar3 = 3;
    do {
      (**(code **)(*piVar2 + 0x1c))(piVar2,puVar7,2);
      puVar7 = puVar7 + 1;
      iVar3 = iVar3 + -1;
    } while (iVar3 != 0);
  }
  FUN_0075fbe8(&DAT_0102a710,(int)DAT_00902ea0);
  if (DAT_0102a713 != '\0') {
    DAT_0102a713 = DAT_0102a713 + '\x05';
  }
  if (DAT_0102a712 != '\0') {
    DAT_0102a712 = DAT_0102a712 + '\x05';
  }
  if (DAT_0102a711 != '\0') {
    DAT_0102a711 = DAT_0102a711 + '\x05';
  }
  if (DAT_0102a710 != '\0') {
    DAT_0102a710 = DAT_0102a710 + '\x05';
  }
  if (0x39ffffffff < lVar8) {
    iVar3 = 0;
    do {
      (**(code **)(*piVar2 + 0x1c))(piVar2,&DAT_0102a704 + iVar3,1);
      iVar3 = iVar3 + 1;
    } while (iVar3 < 4);
    puVar7 = &DAT_0102a708;
    iVar3 = 3;
    do {
      (**(code **)(*piVar2 + 0x1c))(piVar2,puVar7,2);
      puVar7 = puVar7 + 1;
      iVar3 = iVar3 + -1;
    } while (iVar3 != 0);
  }
  FUN_00760b10();
  DAT_0102a6fc = 0;
  DAT_0102a6f8 = 0;
  DAT_0102a700 = 0;
  DAT_0102a738 = 0;
  DAT_0102a734 = 0;
  DAT_0102a730 = 0;
  DAT_0102a72c = 0;
  DAT_0102a728 = 0;
  DAT_0102a724 = 0;
  DAT_0102a720 = 0;
  DAT_0102a71c = 0;
  (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
  FUN_00733bac(piVar2,uVar5);
  (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
  FUN_006082a4(piVar2,uVar5);
  (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
  iVar3 = 0;
  do {
    FUN_00726440(iVar3 + DAT_009027f0,piVar2,uVar5);
    iVar3 = iVar3 + 0x38;
  } while (iVar3 < 56000);
  (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
  (**(code **)(*piVar2 + 0x1c))(piVar2,local_40,1);
  local_43 = local_40[0];
  iVar3 = FUN_007a7d34();
  if (iVar3 == 0) {
    if (local_43 == '\x01') {
      iVar3 = 0;
      do {
        (**(code **)(*piVar2 + 0x1c))(piVar2,&local_44,1);
        bVar1 = local_44;
        iVar6 = FUN_007a7d34();
        if (iVar6 == 0) {
          FUN_0066c0b8(0xbf800000,iVar3 + DAT_00fe1fa8,bVar1);
          (**(code **)(*piVar2 + 0x1c))(piVar2,DAT_00fe1fa8 + iVar3 + 0x138,4);
          (**(code **)(*piVar2 + 0x1c))(piVar2,DAT_00fe1fa8 + iVar3 + 0x13c,4);
          *(int *)(iVar3 + DAT_00fe1fa8 + 0x148) = (int)*(float *)(iVar3 + DAT_00fe1fa8 + 0x138);
          *(int *)(iVar3 + DAT_00fe1fa8 + 0x14c) = (int)*(float *)(iVar3 + DAT_00fe1fa8 + 0x13c);
          (**(code **)(*piVar2 + 0x1c))(piVar2,iVar3 + DAT_00fe1fa8 + 0x119,1);
          (**(code **)(*piVar2 + 0x1c))(piVar2,DAT_00fe1fa8 + iVar3 + 0x1f2,2);
          (**(code **)(*piVar2 + 0x1c))(piVar2,DAT_00fe1fa8 + iVar3 + 500,2);
        }
        iVar3 = iVar3 + 0x274;
        (**(code **)(*piVar2 + 0x1c))(piVar2,&local_43,1);
      } while (local_43 == '\x01');
    }
    (**(code **)(*piVar2 + 0x1c))(piVar2,auStack_3c,4);
    if (lVar8 < 0x3200000000) {
      uVar5 = FUN_00439fe0(piVar2[0xf],local_13c);
      FUN_007ab02c(&DAT_00fe22e0,uVar5);
      if (0xf < local_128) {
        FUN_00431740(local_13c[0]);
      }
      local_128 = 0xf;
      local_12c = 0;
      local_13c[0] = local_13c[0] & 0xffffff00;
      uVar5 = FUN_00439fe0(piVar2[0xf],local_124);
      FUN_007ab02c(&DAT_00fe2310,uVar5);
      if (0xf < local_110) {
        FUN_00431740(local_124[0]);
      }
      local_110 = 0xf;
      local_114 = 0;
      local_124[0] = local_124[0] & 0xffffff00;
      uVar5 = FUN_00439fe0(piVar2[0xf],local_94);
      FUN_007ab02c(&DAT_00fe2340,uVar5);
      if (0xf < local_80) {
        FUN_00431740(local_94[0]);
      }
      local_80 = 0xf;
      local_84 = 0;
      local_94[0] = local_94[0] & 0xffffff00;
      uVar5 = FUN_00439fe0(piVar2[0xf],local_10c);
      FUN_007ab02c(&DAT_00fe2370,uVar5);
      if (0xf < local_f8) {
        FUN_00431740(local_10c[0]);
      }
      local_f8 = 0xf;
      local_fc = 0;
      local_10c[0] = local_10c[0] & 0xffffff00;
      uVar5 = FUN_00439fe0(piVar2[0xf],local_f4);
      FUN_007ab02c(&DAT_00fe23d0,uVar5);
      if (0xf < local_e0) {
        FUN_00431740(local_f4[0]);
      }
      local_e0 = 0xf;
      local_e4 = 0;
      local_f4[0] = local_f4[0] & 0xffffff00;
      uVar5 = FUN_00439fe0(piVar2[0xf],local_dc);
      FUN_007ab02c(&DAT_00fe29d0,uVar5);
      if (0xf < local_c8) {
        FUN_00431740(local_dc[0]);
      }
      local_c8 = 0xf;
      local_cc = 0;
      local_dc[0] = local_dc[0] & 0xffffff00;
      uVar5 = FUN_00439fe0(piVar2[0xf],local_ac);
      FUN_007ab02c(&DAT_00fe26d0,uVar5);
      if (0xf < local_98) {
        FUN_00431740(local_ac[0]);
      }
      local_98 = 0xf;
      local_9c = 0;
      local_ac[0] = local_ac[0] & 0xffffff00;
      uVar5 = FUN_00439fe0(piVar2[0xf],local_7c);
      FUN_007ab02c(&DAT_00fe33c0,uVar5);
      if (0xf < local_68) {
        FUN_00431740(local_7c[0]);
      }
      local_68 = 0xf;
      local_6c = 0;
      local_7c[0] = local_7c[0] & 0xffffff00;
      uVar5 = FUN_00439fe0(piVar2[0xf],local_c4);
      FUN_007ab02c(&DAT_00fe33f0,uVar5);
      if (0xf < local_b0) {
        FUN_00431740(local_c4[0]);
      }
      local_b0 = 0xf;
      local_b4 = 0;
      local_c4[0] = local_c4[0] & 0xffffff00;
      uVar5 = FUN_00439fe0(piVar2[0xf],local_5c);
      FUN_007ab02c(&DAT_00fe36f0,uVar5);
      if (0xf < local_48) {
        FUN_00431740(local_5c[0]);
      }
    }
    else {
      FUN_006d6c24(piVar2,&DAT_00fe22e0);
      FUN_006d6c24(piVar2,&DAT_00fe2310);
      FUN_006d6c24(piVar2,&DAT_00fe2340);
      FUN_006d6c24(piVar2,&DAT_00fe2370);
      FUN_006d6c24(piVar2,&DAT_00fe23d0);
      FUN_006d6c24(piVar2,&DAT_00fe29d0);
      FUN_006d6c24(piVar2,&DAT_00fe26d0);
      FUN_006d6c24(piVar2,&DAT_00fe33c0);
      FUN_006d6c24(piVar2,&DAT_00fe33f0);
      FUN_006d6c24(piVar2,&DAT_00fe36f0);
    }
  }
  FUN_007ffb98();
  return;
}


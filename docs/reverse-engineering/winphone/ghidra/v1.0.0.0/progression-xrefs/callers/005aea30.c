
void FUN_005aea30(void)

{
  wchar_t *pwVar1;
  int iVar2;
  void *pvVar3;
  undefined4 *puVar4;
  undefined4 uVar5;
  undefined4 ****ppppuVar6;
  wchar_t *pwVar7;
  undefined1 auStack_d4 [48];
  undefined4 ***local_a4 [4];
  undefined4 local_94;
  uint local_90;
  undefined4 ***local_8c [4];
  undefined4 local_7c;
  uint local_78;
  undefined1 local_74 [16];
  undefined4 local_64;
  undefined4 local_60;
  undefined1 auStack_44 [48];
  
  FUN_007ffb80();
  DAT_0103396b = 0;
  if (DAT_00fd293c == '\0') {
    FUN_0079ace4(&DAT_01070308);
  }
  else {
    iVar2 = FUN_007a7d34();
    if (iVar2 == 0) {
      FUN_00426e64(local_a4,&DAT_00854800);
      if (DAT_010339bd != '\0') {
        pwVar1 = L".csworld";
        do {
          pwVar7 = pwVar1;
          pwVar1 = pwVar7 + 1;
        } while (*pwVar1 != L'\0');
        FUN_00425974(local_a4,L".csworld",(int)(pwVar7 + -0x42a407) >> 1);
      }
      FUN_00595220(local_74,&DAT_01033788 + (DAT_00902eb4 + -1) * 0x30);
      if (DAT_010339bd != '\0') {
        FUN_007aa80c(local_74,auStack_44,&DAT_01070308);
        FUN_007aa754(auStack_44);
      }
      ppppuVar6 = (undefined4 ****)local_a4[0];
      if (local_90 < 8) {
        ppppuVar6 = local_a4;
      }
      uVar5 = FUN_007aada8(auStack_d4,ppppuVar6);
      FUN_007aacc4(local_74,auStack_44,uVar5);
      FUN_007aa754(auStack_44);
      pvVar3 = malloc(0x40);
      if (pvVar3 == (void *)0x0) {
        puVar4 = (undefined4 *)0x0;
      }
      else {
        *(undefined4 *)((int)pvVar3 + 0x3c) = 0;
        puVar4 = (undefined4 *)FUN_0059dd6c(pvVar3,local_74,0,1,pvVar3);
      }
      FUN_0076a604(puVar4);
      FUN_0058ef48();
      FUN_00735aa0();
      FUN_007aa754(local_74);
      if (7 < local_90) {
        FUN_00431740(local_a4[0]);
      }
      local_90 = 7;
      local_94 = 0;
      local_a4[0] = (undefined4 ***)((uint)local_a4[0] & 0xffff0000);
    }
    else {
      local_60 = 0xf;
      local_64 = 0;
      local_74[0] = 0;
      FUN_0041bc2c(local_74,"Tutorial.world",0xe);
      (**(code **)(*DAT_00ec768c + 0x1c))(DAT_00ec768c,local_8c,local_74);
      ppppuVar6 = (undefined4 ****)local_8c[0];
      if (local_78 < 0x10) {
        ppppuVar6 = local_8c;
      }
      FUN_007ab078(auStack_44,ppppuVar6);
      pvVar3 = malloc(0x40);
      if (pvVar3 == (void *)0x0) {
        puVar4 = (undefined4 *)0x0;
      }
      else {
        *(undefined4 *)((int)pvVar3 + 0x3c) = 0;
        puVar4 = (undefined4 *)FUN_0059dd6c(pvVar3,auStack_44,0,0,pvVar3);
      }
      FUN_0076a604(puVar4);
      FUN_0058ef48();
      FUN_00735aa0();
      FUN_007aa754(auStack_44);
      if (0xf < local_78) {
        FUN_00431740(local_8c[0]);
      }
      local_78 = 0xf;
      local_7c = 0;
      local_8c[0] = (undefined4 ***)((uint)local_8c[0] & 0xffffff00);
    }
    if (puVar4 != (undefined4 *)0x0) {
      (**(code **)*puVar4)(puVar4,1);
    }
    iVar2 = FUN_0058e96c();
    if (*(char *)(iVar2 + 0xa8) == '\0') {
      FUN_0058e96c();
      FUN_005bbb70();
    }
    FUN_0076e360();
    FUN_00758430();
    FUN_0074a7e8();
    FUN_007a9558();
    FUN_005a5788();
    FUN_007b86a8();
  }
  DAT_010338d2 = 1;
  DAT_0103396b = 1;
  iVar2 = FUN_00735520();
  if (iVar2 == 0) {
    DAT_00fe1f86 = 0;
  }
  FUN_007ffb98(0);
  return;
}


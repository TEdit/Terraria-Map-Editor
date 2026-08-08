
longlong FUN_006557e4(undefined4 param_1,uint param_2,undefined4 param_3,undefined4 param_4)

{
  char cVar1;
  char cVar2;
  char cVar3;
  int iVar4;
  int iVar5;
  undefined4 uVar6;
  
  if (((((DAT_00fe1ec4 != 1) && (DAT_010338d3 != '\0')) && (DAT_010703e3 == '\0')) &&
      ((0 < DAT_0102a77c && (DAT_00fe1f71 == '\0')))) &&
     (iVar4 = FUN_00608330(&DAT_00fd67f4,10), cVar3 = DAT_00fe1f8a, cVar2 = DAT_00fe1f89,
     cVar1 = DAT_00fe1f88, iVar4 == 0)) {
    if (((DAT_00fe1f88 == '\0') || (DAT_00fe1f89 == '\0')) || (DAT_00fe1f8a == '\0')) {
      iVar4 = 0;
      do {
        iVar5 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar5 == 0) {
          if (cVar1 == '\0') {
            DAT_00fe1f70 = 1;
            uVar6 = 0xffffffff;
            FUN_00646f20(0x2e,0x32,0xff,0x82,0xffffffff,param_3,param_4);
            return CONCAT44(uVar6,1);
          }
        }
        else if (iVar5 == 1) {
          if (cVar2 == '\0') {
            DAT_00fe1f70 = 2;
            uVar6 = 0xffffffff;
            FUN_00646f20(0x2f,0x32,0xff,0x82,0xffffffff,param_3,param_4);
            return CONCAT44(uVar6,1);
          }
        }
        else if (cVar3 == '\0') {
          DAT_00fe1f70 = 3;
          uVar6 = 0xffffffff;
          FUN_00646f20(0x30,0x32,0xff,0x82,0xffffffff,param_3,param_4);
          return CONCAT44(uVar6,1);
        }
        iVar4 = iVar4 + 1;
      } while (iVar4 < 1000);
    }
  }
  return (ulonglong)param_2 << 0x20;
}


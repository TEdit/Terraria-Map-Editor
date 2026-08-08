
void FUN_007a9278(void)

{
  undefined1 uVar1;
  int iVar2;
  int *piVar3;
  undefined4 uVar4;
  
  uVar1 = DAT_010703e2;
  if (DAT_01033988 + (uint)((ulonglong)DAT_01033988 * (ulonglong)DAT_007a94dc >> 0x26) * -600 == 0)
  {
    FUN_00647e5c();
  }
  iVar2 = FUN_0073517c(&DAT_010703d8);
  if (iVar2 == 0) {
    if (DAT_010703e0 == '\0') {
      if (DAT_010703dc <= DAT_007a94d8) {
        if (DAT_007a94d4 < DAT_010703dc) {
          FUN_006aa98c();
        }
      }
      else if (DAT_0102a780 != '\0') {
        DAT_0102a780 = 0;
        FUN_00783038();
        FUN_00749de0(&DAT_010703d8);
        FUN_007491d4();
        return;
      }
    }
    else if (DAT_00fe1ec4 != 1) {
      FUN_00697314();
      FUN_00749de0(&DAT_010703d8);
      FUN_007491d4();
      return;
    }
  }
  else {
    DAT_00fe1f6c = 0;
    DAT_00fe1f78 = 0;
    if (DAT_010703e0 == '\0') {
      if (DAT_00fe1ec4 != 1) {
        if ((DAT_0102a76f != '\0') && (iVar2 = FUN_00608330(&DAT_00fd67f4,0x32), iVar2 == 0)) {
          DAT_0102a780 = '\x01';
        }
        iVar2 = FUN_006597c4();
        if (((iVar2 == 0) && (DAT_010703e4 != '\x04')) &&
           (iVar2 = FUN_00608330(&DAT_00fd67f4,9), iVar2 == 0)) {
          iVar2 = 0;
          piVar3 = &DAT_0107034c;
          do {
            if ((*(char *)(*piVar3 + 0x5cc5) != '\0') && (0x78 < *(short *)(*piVar3 + 0x5d00))) {
              FUN_00735020(&DAT_010703d8);
              break;
            }
            iVar2 = iVar2 + 1;
            piVar3 = piVar3 + 1;
          } while (iVar2 < 4);
        }
        iVar2 = 4;
        piVar3 = &DAT_0107034c;
        do {
          if (*(char *)(*piVar3 + 0x5cc5) != '\0') {
            FUN_006d289c(*piVar3,0);
          }
          iVar2 = iVar2 + -1;
          piVar3 = piVar3 + 1;
        } while (iVar2 != 0);
      }
    }
    else {
      if (0 < DAT_010339ac) {
        DAT_010339ac = DAT_010339ac + -1;
      }
      if (DAT_00fe1ec4 != 1) {
        if (DAT_0102a76f != '\0') {
          if (DAT_00fe1f83 == '\0') {
            uVar4 = 3;
          }
          else if (DAT_010338d3 == '\0') {
            uVar4 = 0x1e;
          }
          else {
            uVar4 = 0x3c;
          }
          iVar2 = FUN_00608330(&DAT_00fd67f4,uVar4);
          if (iVar2 == 0) {
            FUN_007a8f74(1);
          }
          if ((DAT_010339a0 == 0) && (DAT_010338d3 != '\0')) {
            if (DAT_00fe1f87 == '\0') {
              uVar4 = 0x28;
            }
            else {
              uVar4 = 0x3c;
            }
            iVar2 = FUN_00608330(&DAT_00fd67f4,uVar4);
            if (iVar2 == 0) {
              FUN_007a8f74(3);
            }
          }
        }
        if (((DAT_010338d3 != '\0') && (DAT_00fe1f8b != '\0')) &&
           (iVar2 = FUN_00608330(&DAT_00fd67f4,0x19), iVar2 == 0)) {
          FUN_00735048(&DAT_010703d8);
        }
        iVar2 = 4;
        piVar3 = &DAT_0107034c;
        do {
          if (*(char *)(*piVar3 + 0x5cc5) != '\0') {
            FUN_006d289c(*piVar3,uVar1);
          }
          iVar2 = iVar2 + -1;
          piVar3 = piVar3 + 1;
        } while (iVar2 != 0);
      }
    }
    if (DAT_00fe1ec4 == 2) {
      FUN_00647dc0(0);
      FUN_00749de0(&DAT_010703d8);
      FUN_007491d4();
      return;
    }
  }
  FUN_00749de0(&DAT_010703d8);
  FUN_007491d4();
  return;
}


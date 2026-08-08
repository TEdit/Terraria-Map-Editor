
void FUN_00606bc4(int param_1,undefined4 param_2,int param_3)

{
  byte bVar1;
  int iVar2;
  undefined4 uVar3;
  int iVar4;
  int iVar5;
  int iVar6;
  uint in_fpscr;
  uint uVar7;
  float fVar8;
  float fVar9;
  float fVar10;
  
  iVar2 = FUN_00592fc8();
  fVar10 = *(float *)(iVar2 + 0x5288);
  iVar6 = param_1 + 0x1900;
  iVar2 = 0x28;
  do {
    iVar4 = iVar2;
    iVar6 = iVar6 + -0xa0;
    iVar2 = iVar4 + -1;
    FUN_00612a6c(iVar6);
  } while (0 < iVar2);
  switch(param_2) {
  case 1:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x58,1,0);
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x57,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x23,1,0);
    FUN_006317fc(param_1 + (iVar4 + 2) * 0xa0,0xfffffff3,1);
    FUN_006317fc(param_1 + (iVar4 + 3) * 0xa0,0xfffffff0,1);
    FUN_00616ed8(param_1 + (iVar4 + 4) * 0xa0,8,1,0);
    iVar6 = iVar4 + 6;
    FUN_00616ed8(param_1 + (iVar4 + 5) * 0xa0,0x1c,1,0);
    if (param_3 != 0) {
      iVar2 = FUN_006c1308();
      fVar10 = (float)VectorSignedToFloat((int)*(short *)(iVar2 + 0x5d0c),
                                          (byte)(in_fpscr >> 0x16) & 3);
      fVar8 = (float)VectorSignedToFloat((int)*(short *)(param_3 + 0x5d08),
                                         (byte)(in_fpscr >> 0x16) & 3);
      if (fVar10 * 0.25 <= fVar8) {
        iVar2 = iVar6 * 0xa0;
        iVar6 = iVar4 + 7;
        FUN_00616ed8(param_1 + iVar2,0x6e,1,0);
      }
    }
    FUN_00616ed8(param_1 + iVar6 * 0xa0,0x28,1,0);
    FUN_00616ed8(param_1 + (iVar6 + 1) * 0xa0,0x2a,1,0);
    iVar2 = iVar6 + 3;
    FUN_00616ed8(param_1 + (iVar6 + 2) * 0xa0,0x3c5,1,0);
    if ((param_3 != 0) && (*(char *)(param_3 + 0xea) != '\0')) {
      iVar4 = iVar2 * 0xa0;
      iVar2 = iVar6 + 4;
      FUN_00616ed8(param_1 + iVar4,0x3c7,1,0);
    }
    if (DAT_010703e2 != '\0') {
      iVar6 = iVar2 * 0xa0;
      iVar2 = iVar2 + 1;
      FUN_00616ed8(param_1 + iVar6,0x117,1,0);
    }
    if (DAT_010703e0 == '\0') {
      iVar6 = iVar2 * 0xa0;
      iVar2 = iVar2 + 1;
      FUN_00616ed8(param_1 + iVar6,0x11a,1,0);
    }
    if (DAT_00fe1f7b != '\0') {
      iVar6 = iVar2 * 0xa0;
      iVar2 = iVar2 + 1;
      FUN_00616ed8(param_1 + iVar6,0x15a,1,0);
    }
    if (DAT_010338d3 != '\0') {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x1e8,1,0);
    }
    if ((param_3 != 0) && (iVar6 = FUN_006beaa8(param_3,0x3a2), iVar6 != 0)) {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x3a3,1,0);
      iVar6 = iVar2 + 1;
      iVar2 = iVar2 + 2;
      FUN_00616ed8(param_1 + iVar6 * 0xa0,0x64e,1,0);
    }
    iVar6 = FUN_007354a8();
    if (iVar6 != 0) {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x6fa,1,0);
      iVar2 = iVar2 + 1;
    }
    if (DAT_010338d3 != '\0') {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x544,1,0);
      iVar2 = iVar2 + 1;
    }
    iVar6 = FUN_00735594();
    if (iVar6 != 0) {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x13bb,1,0);
      iVar6 = iVar2 + 1;
      iVar2 = iVar2 + 2;
      FUN_00616ed8(param_1 + iVar6 * 0xa0,0x13b9,1,0);
    }
    iVar6 = FUN_00735468();
    if (iVar6 != 0) {
      iVar6 = iVar2 * 0xa0;
      iVar2 = iVar2 + 1;
      FUN_00616ed8(param_1 + iVar6,0x13c7,1,0);
    }
    iVar6 = FUN_007354e4();
    if (iVar6 == 0) goto switchD_00606c02_default;
    uVar3 = 0x13d2;
    iVar6 = iVar2 + 1;
    iVar5 = iVar2;
    break;
  case 2:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x61,1,0);
    if ((DAT_010703e2 != '\0') || (DAT_010338d3 != '\0')) {
      iVar2 = iVar4 * 0xa0;
      iVar4 = iVar4 + 1;
      FUN_00616ed8(param_1 + iVar2,0x116,1,0);
    }
    if (((DAT_00fe1f7a != '\0') && (DAT_010703e0 == '\0')) || (DAT_010338d3 != '\0')) {
      iVar2 = iVar4 * 0xa0;
      iVar4 = iVar4 + 1;
      FUN_00616ed8(param_1 + iVar2,0x2f,1,0);
    }
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x5f,1,0);
    iVar2 = iVar4 + 2;
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x62,1,0);
    if (DAT_010703e0 == '\0') {
      iVar6 = iVar2 * 0xa0;
      iVar2 = iVar4 + 3;
      FUN_00616ed8(param_1 + iVar6,0x144,1,0);
    }
    if (DAT_010338d3 != '\0') {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x216,1,0);
      iVar6 = iVar2 + 1;
      iVar2 = iVar2 + 2;
      FUN_00616ed8(param_1 + iVar6 * 0xa0,0x598,1,0);
    }
    if (param_3 != 0) {
      iVar6 = FUN_006beaa8(param_3,0x4ea);
      if (iVar6 != 0) {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x4ed,1,0);
      }
      iVar6 = FUN_006beaa8(param_3,0x72b);
      if (iVar6 != 0) {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x72c,1,0);
      }
      iVar6 = FUN_006beaa8(param_3,0x6f6);
      if (iVar6 != 0) {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x6f7,1,0);
      }
      iVar6 = FUN_006beaa8(param_3,0x6f8);
      if (iVar6 != 0) {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x6f9,1,0);
      }
    }
    iVar6 = FUN_007354a8();
    if (iVar6 == 0) goto switchD_00606c02_default;
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x6c8,1,0);
    FUN_00616ed8(param_1 + (iVar2 + 1) * 0xa0,0x6c9,1,0);
    uVar3 = 0x6ca;
    iVar6 = iVar2 + 3;
    iVar5 = iVar2 + 2;
    break;
  case 3:
    iVar2 = param_1 + iVar2 * 0xa0;
    if (DAT_010703e2 == '\0') {
      FUN_00616ed8(iVar2,0x42,1,0);
      FUN_00616ed8(param_1 + iVar4 * 0xa0,0x3e,1,0);
      iVar2 = iVar4 + 1;
      iVar6 = iVar4 + 2;
      uVar3 = 0x3f;
    }
    else {
      FUN_00616ed8(iVar2,0x43,1,0);
      iVar6 = iVar4 + 1;
      uVar3 = 0x3b;
      iVar2 = iVar4;
    }
    FUN_00616ed8(param_1 + iVar2 * 0xa0,uVar3,1,0);
    FUN_00616ed8(param_1 + iVar6 * 0xa0,0x1b,1,0);
    FUN_00616ed8(param_1 + (iVar6 + 1) * 0xa0,0x72,1,0);
    FUN_00616ed8(param_1 + (iVar6 + 2) * 0xa0,0x724,1,0);
    FUN_00616ed8(param_1 + (iVar6 + 3) * 0xa0,0x2e9,1,0);
    iVar2 = iVar6 + 5;
    FUN_00616ed8(param_1 + (iVar6 + 4) * 0xa0,0x2eb,1,0);
    if (DAT_010338d3 != '\0') {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x2ea,1,0);
      iVar2 = iVar6 + 7;
      FUN_00616ed8(param_1 + (iVar6 + 6) * 0xa0,0x171,1,0);
      if ((DAT_010703e0 == '\0') && ((DAT_010703e4 == 4 || (DAT_010703e2 != '\0')))) {
        iVar4 = iVar2 * 0xa0;
        iVar2 = iVar6 + 8;
        FUN_00616ed8(param_1 + iVar4,0x13ab,1,0);
      }
    }
    if ((param_3 != 0) && (iVar6 = FUN_00592fc8(), 10 < *(int *)(iVar6 + 0x4cc0))) {
      iVar6 = iVar2 * 0xa0;
      iVar2 = iVar2 + 1;
      FUN_00616ed8(param_1 + iVar6,0xc2,1,0);
    }
    iVar6 = FUN_007354a8();
    if (iVar6 == 0) goto switchD_00606c02_default;
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x73d,1,0);
    uVar3 = 0x73e;
    goto LAB_006081e4;
  case 4:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0xa8,1,0);
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0xa6,1,0);
    iVar2 = iVar4 + 2;
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0xa7,1,0);
    if (DAT_010338d3 == '\0') goto switchD_00606c02_default;
    iVar5 = iVar4 + 3;
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x109,1,0);
    if (DAT_00fe1f8d != '\0') {
      iVar2 = iVar5 * 0xa0;
      iVar5 = iVar4 + 4;
      FUN_00616ed8(param_1 + iVar2,0x3a9,1,0);
    }
    uVar3 = 0x543;
    iVar6 = iVar5 + 1;
    break;
  case 5:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0xfe,1,0);
    iVar2 = iVar4 + 1;
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x3d5,1,0);
    if (DAT_010703e0 != '\0') {
      iVar6 = iVar2 * 0xa0;
      iVar2 = iVar4 + 2;
      FUN_00616ed8(param_1 + iVar6,0xf2,1,0);
    }
    if (DAT_010703e4 == 0) {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0xf5,1,0);
      uVar3 = 0xf6;
LAB_00607326:
      iVar6 = iVar2 + 1;
      iVar2 = iVar2 + 2;
      FUN_00616ed8(param_1 + iVar6 * 0xa0,uVar3,1,0);
    }
    else if (DAT_010703e4 == 1) {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x145,1,0);
      uVar3 = 0x146;
      goto LAB_00607326;
    }
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x10d,1,0);
    FUN_00616ed8(param_1 + (iVar2 + 1) * 0xa0,0x10e,1,0);
    iVar6 = iVar2 + 3;
    FUN_00616ed8(param_1 + (iVar2 + 2) * 0xa0,0x10f,1,0);
    if (DAT_00fe1f85 != '\0') {
      FUN_00616ed8(param_1 + iVar6 * 0xa0,0x1f7,1,0);
      FUN_00616ed8(param_1 + (iVar2 + 4) * 0xa0,0x1f8,1,0);
      iVar6 = iVar2 + 6;
      FUN_00616ed8(param_1 + (iVar2 + 5) * 0xa0,0x1f9,1,0);
    }
    iVar4 = iVar6;
    if (DAT_010703e2 != '\0') {
      iVar4 = iVar6 + 1;
      FUN_00616ed8(param_1 + iVar6 * 0xa0,0x142,1,0);
      switch(DAT_010703e4) {
      case 2:
      case 5:
        if ((param_3 == 0) || (*(char *)(param_3 + 0xdb) != '\0')) {
          iVar2 = iVar4 * 0xa0;
          iVar4 = iVar6 + 2;
          FUN_00616ed8(param_1 + iVar2,0x13a6,1,0);
        }
        if ((param_3 == 0) || (*(char *)(param_3 + 0xdb) == '\0')) {
          uVar3 = 0x13a5;
LAB_00607472:
          iVar2 = iVar4 * 0xa0;
          iVar4 = iVar4 + 1;
          FUN_00616ed8(param_1 + iVar2,uVar3,1,0);
        }
        break;
      case 3:
      case 7:
        if ((param_3 == 0) || (*(char *)(param_3 + 0xdb) != '\0')) {
          iVar2 = iVar4 * 0xa0;
          iVar4 = iVar6 + 2;
          FUN_00616ed8(param_1 + iVar2,0x13aa,1,0);
        }
        if ((param_3 == 0) || (*(char *)(param_3 + 0xdb) == '\0')) {
          uVar3 = 0x13a9;
          goto LAB_00607472;
        }
        break;
      case 6:
        if ((param_3 == 0) || (*(char *)(param_3 + 0xdb) != '\0')) {
          iVar2 = iVar4 * 0xa0;
          iVar4 = iVar6 + 2;
          FUN_00616ed8(param_1 + iVar2,0x13a8,1,0);
        }
        if ((param_3 == 0) || (*(char *)(param_3 + 0xdb) == '\0')) {
          uVar3 = 0x13a7;
          goto LAB_00607472;
        }
      }
    }
    if ((param_3 != 0) && (*(char *)(param_3 + 0xea) != '\0')) {
      iVar2 = iVar4 * 0xa0;
      iVar4 = iVar4 + 1;
      FUN_00616ed8(param_1 + iVar2,0x595,1,0);
    }
    iVar2 = FUN_007354a8();
    if (iVar2 != 0) {
      iVar2 = iVar4 * 0xa0;
      iVar4 = iVar4 + 1;
      FUN_00616ed8(param_1 + iVar2,0x6cc,1,0);
    }
    iVar2 = iVar4;
    if (DAT_010338d3 != '\0') {
      if (DAT_010703e4 == 2) {
        iVar2 = iVar4 + 1;
        uVar3 = 0x365;
      }
      else if (DAT_010703e4 == 4) {
        FUN_00616ed8(param_1 + iVar4 * 0xa0,0x360,1,0);
        iVar2 = iVar4 + 2;
        uVar3 = 0x361;
        iVar4 = iVar4 + 1;
      }
      else {
        if (DAT_010703e4 != 6) goto LAB_00607548;
        FUN_00616ed8(param_1 + iVar4 * 0xa0,0x369,1,0);
        FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x36a,1,0);
        iVar2 = iVar4 + 3;
        uVar3 = 0x36b;
        iVar4 = iVar4 + 2;
      }
      FUN_00616ed8(param_1 + iVar4 * 0xa0,uVar3,1,0);
    }
LAB_00607548:
    if (DAT_00fe1f84 == '\0') goto switchD_00606c02_default;
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x4fb,1,0);
    uVar3 = 0x4fc;
    goto LAB_006081e4;
  case 6:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x80,1,0);
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x18e,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x54,1,0);
    uVar3 = 0xa1;
    iVar6 = iVar4 + 3;
    iVar5 = iVar4 + 2;
    break;
  case 7:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x1e7,1,0);
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x1f0,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,500,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 2) * 0xa0,0x1fb,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 3) * 0xa0,0x1fc,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 4) * 0xa0,0x213,1,0);
    iVar2 = iVar4 + 6;
    FUN_00616ed8(param_1 + (iVar4 + 5) * 0xa0,0x240,1,0);
    iVar6 = FUN_007354a8();
    if (iVar6 == 0) goto switchD_00606c02_default;
    uVar3 = 0x6cb;
    iVar6 = iVar4 + 7;
    iVar5 = iVar2;
    break;
  case 8:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x1fd,1,0);
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x1fe,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x212,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 2) * 0xa0,0x201,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 3) * 0xa0,0x21a,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 4) * 0xa0,0x247,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 5) * 0xa0,0x248,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 6) * 0xa0,0x249,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 7) * 0xa0,0x211,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 8) * 0xa0,0x21d,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 9) * 0xa0,0x21e,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 10) * 0xa0,0x21f,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 0xb) * 0xa0,0x354,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 0xc) * 0xa0,0x355,1,0);
    uVar3 = 0x351;
    iVar6 = iVar4 + 0xe;
    iVar5 = iVar4 + 0xd;
    break;
  case 9:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x24c,1,0);
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x24d,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x24e,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 2) * 0xa0,0x255,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 3) * 0xa0,0x256,1,0);
    uVar3 = 0x254;
    iVar6 = iVar4 + 5;
    iVar5 = iVar4 + 4;
    break;
  case 10:
    iVar6 = iVar2;
    if (DAT_00fe1f8b != '\0') {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x2f4,1,0);
      iVar6 = iVar4 + 1;
      FUN_00616ed8(param_1 + iVar4 * 0xa0,0x313,1,0);
    }
    iVar2 = iVar6 + 1;
    FUN_00616ed8(param_1 + iVar6 * 0xa0,0x364,1,0);
    if (DAT_00fe1f8d != '\0') {
      iVar4 = iVar2 * 0xa0;
      iVar2 = iVar6 + 2;
      FUN_00616ed8(param_1 + iVar4,0x60f,1,0);
    }
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x49d,1,0);
    uVar3 = 0x30f;
    goto LAB_006081e4;
  case 0xb:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x30b,1,0);
    if (DAT_010703e4 < 4) {
      FUN_00616ed8(param_1 + iVar4 * 0xa0,0x347,1,0);
      FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x348,1,0);
      iVar2 = iVar4 + 2;
      iVar6 = iVar4 + 3;
      uVar3 = 0x349;
    }
    else {
      iVar6 = iVar4 + 1;
      uVar3 = 0x2ec;
      iVar2 = iVar4;
    }
    FUN_00616ed8(param_1 + iVar2 * 0xa0,uVar3,1,0);
    if (DAT_010703e0 == '\0') {
      uVar3 = 0x3e3;
    }
    else {
      uVar3 = 0x3e6;
    }
    FUN_00616ed8(param_1 + iVar6 * 0xa0,uVar3,1,0);
    FUN_00616ed8(param_1 + (iVar6 + 1) * 0xa0,0x4ef,1,0);
    if ((DAT_010703e1 == '\0') && (DAT_010703e2 == '\0')) {
      if ((param_3 == 0) || (*(char *)(param_3 + 0xe7) == '\0')) {
        uVar3 = 0x30c;
      }
      else {
        uVar3 = 0x30d;
      }
    }
    else if (DAT_0102a787 == '\0') {
      uVar3 = 0x30e;
    }
    else {
      uVar3 = 0x310;
    }
    iVar2 = iVar6 + 3;
    FUN_00616ed8(param_1 + (iVar6 + 2) * 0xa0,uVar3,1,0);
    if (DAT_010338d3 != '\0') {
      iVar4 = iVar2 * 0xa0;
      iVar2 = iVar6 + 4;
      FUN_00616ed8(param_1 + iVar4,0x540,1,0);
    }
    iVar6 = FUN_007354a8();
    if (iVar6 == 0) goto switchD_00606c02_default;
    uVar3 = 0x6ce;
    iVar6 = iVar2 + 1;
    iVar5 = iVar2;
    break;
  case 0xc:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x40d,1,0);
    iVar2 = iVar4 + 1;
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x460,1,0);
    iVar6 = FUN_007354a8();
    if (iVar6 == 0) goto switchD_00606c02_default;
    uVar3 = 0x6cd;
    iVar6 = iVar4 + 2;
    iVar5 = iVar2;
    break;
  case 0xd:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,1000,1,0);
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x490,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x5a9,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 2) * 0xa0,0x541,1,0);
    iVar2 = iVar4 + 4;
    FUN_00616ed8(param_1 + (iVar4 + 3) * 0xa0,0x5aa,1,0);
    if (DAT_010338d3 == '\0') goto switchD_00606c02_default;
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x3ca,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 5) * 0xa0,0x3cb,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 6) * 0xa0,0x3cc,1,0);
    uVar3 = 0x3cd;
    iVar6 = iVar4 + 8;
    iVar5 = iVar4 + 7;
    break;
  case 0xe:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x303,1,0);
    if (DAT_010703e2 != '\0') {
      iVar2 = iVar4 * 0xa0;
      iVar4 = iVar4 + 1;
      FUN_00616ed8(param_1 + iVar2,0x304,1,0);
    }
    if ((DAT_010703e0 == '\0') || (iVar6 = iVar4, DAT_010703e1 != '\0')) {
      iVar6 = iVar4 + 1;
      FUN_00616ed8(param_1 + iVar4 * 0xa0,0x305,1,0);
      if (DAT_010703e1 != '\0') {
        iVar2 = iVar6 * 0xa0;
        iVar6 = iVar4 + 2;
        FUN_00616ed8(param_1 + iVar2,0x306,1,0);
      }
    }
    iVar2 = iVar6;
    if (DAT_010338d3 != '\0') {
      iVar2 = iVar6 + 1;
      FUN_00616ed8(param_1 + iVar6 * 0xa0,0x2f8,1,0);
      if (DAT_010338d3 != '\0') {
        iVar4 = iVar2 * 0xa0;
        iVar2 = iVar6 + 2;
        FUN_00616ed8(param_1 + iVar4,0x542,1,0);
      }
    }
    iVar6 = FUN_007354a8();
    if (iVar6 == 0) goto switchD_00606c02_default;
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x6cf,1,0);
    FUN_00616ed8(param_1 + (iVar2 + 1) * 0xa0,0x6d0,1,0);
    uVar3 = 0x6d1;
    iVar6 = iVar2 + 3;
    iVar5 = iVar2 + 2;
    break;
  case 0xf:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x42f,1,0);
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x430,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x44c,1,0);
    iVar6 = 0x431;
    iVar2 = iVar4 + 2;
    do {
      iVar4 = iVar2;
      FUN_00616ed8(param_1 + iVar4 * 0xa0,iVar6,1,0);
      iVar6 = iVar6 + 1;
      iVar2 = iVar4 + 1;
    } while (iVar6 < 0x43d);
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x449,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 2) * 0xa0,1099,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 3) * 0xa0,0x44a,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 4) * 0xa0,0x5d2,1,0);
    if (DAT_010703e4 < 2) {
      uVar3 = 0x5c9;
    }
    else if (DAT_010703e4 < 4) {
      uVar3 = 0x5ca;
    }
    else if (DAT_010703e4 < 6) {
      uVar3 = 0x5cb;
    }
    else {
      uVar3 = 0x5cc;
    }
    iVar2 = iVar4 + 6;
    FUN_00616ed8(param_1 + (iVar4 + 5) * 0xa0,uVar3,1,0);
    if (param_3 != 0) {
      if (*(char *)(param_3 + 0xeb) != '\0') {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar4 + 7;
        FUN_00616ed8(param_1 + iVar6,0x5d4,1,0);
      }
      if (*(char *)(param_3 + 0xe6) != '\0') {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x5d0,1,0);
      }
      if (*(char *)(param_3 + 0xe7) != '\0') {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x5d1,1,0);
      }
      if (*(char *)(param_3 + 0xe9) != '\0') {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x5ce,1,0);
      }
      if (*(char *)(param_3 + 0xea) != '\0') {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x5cf,1,0);
      }
      iVar6 = FUN_00592fc8();
      fVar10 = fVar10 * DAT_00607e70;
      fVar8 = (float)VectorSignedToFloat(*(undefined4 *)(iVar6 + 0x4c9c),
                                         (byte)(in_fpscr >> 0x16) & 3);
      uVar7 = in_fpscr & 0xfffffff | (uint)(fVar8 < fVar10) << 0x1f |
              (uint)(fVar8 == fVar10) << 0x1e;
      in_fpscr = uVar7 | (uint)(NAN(fVar8) || NAN(fVar10)) << 0x1c;
      bVar1 = (byte)(uVar7 >> 0x18);
      if (!(bool)(bVar1 >> 6 & 1) && bVar1 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x5d3,1,0);
      }
    }
    if (DAT_010703e2 != '\0') {
      iVar6 = iVar2 * 0xa0;
      iVar2 = iVar2 + 1;
      FUN_00616ed8(param_1 + iVar6,0x5d5,1,0);
    }
    fVar8 = DAT_00607e6c;
    fVar10 = DAT_00607e68;
    if (param_3 == 0) {
      return;
    }
    fVar9 = (float)VectorSignedToFloat(DAT_00902ebc,(byte)(in_fpscr >> 0x16) & 3);
    uVar7 = in_fpscr & 0xfffffff |
            (uint)(fVar9 * DAT_00607e68 <= *(float *)(param_3 + 0x114) * DAT_00607e6c) << 0x1d;
    if ((byte)(uVar7 >> 0x1d) == 0) {
      iVar6 = iVar2 * 0xa0;
      iVar2 = iVar2 + 1;
      FUN_00616ed8(param_1 + iVar6,0x5cd,1,0);
    }
    fVar9 = (float)VectorSignedToFloat(DAT_00902ebc,(byte)(uVar7 >> 0x16) & 3);
    if ((fVar9 * fVar10 <= *(float *)(param_3 + 0x114) * fVar8) || (DAT_010338d3 == '\0'))
    goto switchD_00606c02_default;
    uVar3 = 0x5d6;
    iVar6 = iVar2 + 1;
    iVar5 = iVar2;
    break;
  case 0x10:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x596,1,0);
    iVar6 = iVar4 + 1;
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x3da,1,0);
    if ((DAT_010338d3 != '\0') && (DAT_00fe1f8d != '\0')) {
      if ((param_3 != 0) && (iVar2 = FUN_006beaa8(param_3,0x485), iVar2 != 0)) {
        FUN_00616ed8(param_1 + iVar6 * 0xa0,0x487,1,0);
        FUN_00616ed8(param_1 + (iVar4 + 2) * 0xa0,0x488,1,0);
        iVar6 = iVar4 + 4;
        FUN_00616ed8(param_1 + (iVar4 + 3) * 0xa0,0x489,1,0);
        if (DAT_010703e0 == '\0') {
          iVar2 = iVar6 * 0xa0;
          iVar6 = iVar4 + 5;
          FUN_00616ed8(param_1 + iVar2,0x486,1,0);
        }
        if (*(char *)(param_3 + 0xe9) != '\0') {
          iVar2 = iVar6 * 0xa0;
          iVar6 = iVar6 + 1;
          FUN_00616ed8(param_1 + iVar2,0x48f,1,0);
        }
      }
      iVar2 = iVar6 * 0xa0;
      iVar6 = iVar6 + 1;
      FUN_00616ed8(param_1 + iVar2,0x53b,1,0);
    }
    iVar2 = iVar6;
    if (((DAT_010338d3 != '\0') && (param_3 != 0)) && (*(char *)(param_3 + 0xe9) != '\0')) {
      iVar2 = iVar6 + 1;
      FUN_00616ed8(param_1 + iVar6 * 0xa0,0x493,1,0);
      if (DAT_010703e0 == '\0') {
        iVar4 = iVar2 * 0xa0;
        iVar2 = iVar6 + 2;
        FUN_00616ed8(param_1 + iVar4,0x48a,1,0);
      }
    }
    if ((DAT_010338d3 != '\0') && (DAT_00fe1f8d != '\0')) {
      FUN_00616ed8(param_1 + iVar2 * 0xa0,0x38d,1,0);
      FUN_00616ed8(param_1 + (iVar2 + 1) * 0xa0,0x38e,1,0);
      FUN_00616ed8(param_1 + (iVar2 + 2) * 0xa0,0x3ac,1,0);
      FUN_00616ed8(param_1 + (iVar2 + 3) * 0xa0,0x3ad,1,0);
      FUN_00616ed8(param_1 + (iVar2 + 4) * 0xa0,0x3ae,1,0);
      FUN_00616ed8(param_1 + (iVar2 + 5) * 0xa0,0x3af,1,0);
      FUN_00616ed8(param_1 + (iVar2 + 6) * 0xa0,0x3b0,1,0);
      iVar6 = iVar2 + 7;
      iVar2 = iVar2 + 8;
      FUN_00616ed8(param_1 + iVar6 * 0xa0,0x3b1,1,0);
    }
    if (param_3 != 0) {
      iVar6 = FUN_006beaa8(param_3,0x72b);
      if (iVar6 != 0) {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x72c,1,0);
      }
      iVar6 = FUN_006beaa8(param_3,0x4ea);
      if (iVar6 != 0) {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar2 + 1;
        FUN_00616ed8(param_1 + iVar6,0x4ed,1,0);
      }
    }
    iVar6 = FUN_007354a8();
    if (iVar6 == 0) goto switchD_00606c02_default;
    uVar3 = 0x6ff;
    iVar6 = iVar2 + 1;
    iVar5 = iVar2;
    break;
  case 0x11:
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x3a0,1,0);
    FUN_00616ed8(param_1 + iVar4 * 0xa0,0x3a1,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 1) * 0xa0,0x36c,1,0);
    FUN_00616ed8(param_1 + (iVar4 + 2) * 0xa0,0x36d,1,0);
    iVar2 = iVar4 + 4;
    FUN_00616ed8(param_1 + (iVar4 + 3) * 0xa0,0x36e,1,0);
    if (param_3 != 0) {
      iVar6 = FUN_00592fc8();
      iVar5 = *(int *)(iVar6 + 0x10c) + DAT_00902e84 / 2;
      iVar5 = (int)(iVar5 + ((uint)(iVar5 >> 3) >> 0x1c)) >> 4;
      if (((int)(*(int *)(iVar6 + 0x110) + ((uint)(*(int *)(iVar6 + 0x110) >> 3) >> 0x1c)) >> 4 <
           DAT_00902ebc + 10) && ((iVar5 < 0x17c || (DAT_00902ea0 + -0x17c < iVar5)))) {
        iVar6 = iVar2 * 0xa0;
        iVar2 = iVar4 + 5;
        FUN_00616ed8(param_1 + iVar6,0x49c,1,0);
      }
    }
    if (((DAT_010338d3 == '\0') || (DAT_00fe1f8b == '\0')) ||
       (iVar6 = FUN_00656370(0xd0), iVar6 == 0)) goto switchD_00606c02_default;
    FUN_00616ed8(param_1 + iVar2 * 0xa0,0x539,1,0);
    uVar3 = 0x53a;
LAB_006081e4:
    iVar6 = iVar2 + 2;
    iVar5 = iVar2 + 1;
    break;
  default:
    goto switchD_00606c02_default;
  }
  iVar2 = iVar6;
  FUN_00616ed8(param_1 + iVar5 * 0xa0,uVar3,1,0);
switchD_00606c02_default:
  iVar6 = DAT_00608228;
  if (((param_3 != 0) && (*(char *)(param_3 + 0x47) != '\0')) && (0 < iVar2)) {
    do {
      iVar4 = (int)((ulonglong)((longlong)(*(int *)(param_1 + 0x98) << 3) * (longlong)iVar6) >> 0x20
                   );
      *(int *)(param_1 + 0x98) = (iVar4 >> 2) - (iVar4 >> 0x1f);
      param_1 = param_1 + 0xa0;
      iVar2 = iVar2 + -1;
    } while (iVar2 != 0);
  }
  return;
}


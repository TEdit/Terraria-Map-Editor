
void FUN_006a47dc(void)

{
  int iVar1;
  code *pcVar2;
  short sVar3;
  byte bVar4;
  char cVar5;
  int iVar6;
  int iVar7;
  int iVar8;
  uint uVar9;
  uint uVar10;
  uint uVar11;
  uint uVar12;
  uint uVar13;
  undefined2 uVar14;
  uint uVar15;
  uint uVar16;
  undefined4 uVar17;
  undefined4 uVar18;
  int iVar19;
  uint uVar20;
  uint uVar21;
  int iVar22;
  int iVar23;
  int iVar24;
  bool bVar25;
  uint in_fpscr;
  float fVar26;
  float fVar27;
  float fVar28;
  float fVar29;
  float fVar30;
  undefined4 uVar31;
  int local_1a4;
  uint local_19c;
  undefined1 auStack_18c [24];
  undefined1 auStack_174 [24];
  undefined1 auStack_15c [24];
  undefined1 auStack_144 [24];
  undefined1 auStack_12c [24];
  undefined1 auStack_114 [24];
  undefined1 auStack_fc [48];
  undefined1 auStack_cc [48];
  undefined4 local_9c;
  int local_98;
  undefined4 local_94;
  undefined4 local_90;
  undefined1 auStack_8c [104];
  
  iVar6 = FUN_007ffb80();
  cVar5 = DAT_00fe1f8b;
  if (DAT_00fe1ec4 == 1) goto LAB_006aa94a;
  iVar7 = FUN_006c314c(iVar6 + 0x148);
  if (DAT_010338d3 != '\0') {
    if ((*(int *)(iVar6 + 0x104) == 0x79) || (*(int *)(iVar6 + 0x104) == 0x17)) {
      bVar25 = false;
    }
    else if (((*(int *)(iVar6 + 0x1b8) < 2) || (*(int *)(iVar6 + 0x19c) < 1)) ||
            (*(char *)(iVar6 + 0x11a) != '\0')) {
      bVar25 = false;
    }
    else {
      bVar25 = true;
    }
    if (bVar25) {
      fVar26 = *(float *)(iVar6 + 0x1ec);
      uVar15 = in_fpscr & 0xfffffff | (uint)(fVar26 < 0.0) << 0x1f | (uint)(fVar26 == 0.0) << 0x1e;
      in_fpscr = uVar15 | (uint)NAN(fVar26) << 0x1c;
      bVar4 = (byte)(uVar15 >> 0x18);
      if (((!(bool)(bVar4 >> 6 & 1) && bVar4 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) &&
          (iVar8 = FUN_00608330(&DAT_00fd67f4,5), iVar8 == 0)) &&
         ((DAT_0103395c < *(int *)(iVar6 + 0x14c) ||
          (iVar8 = FUN_00608330(&DAT_01070468,6), iVar8 == 0)))) {
        if (*(char *)(iVar7 + 0xe6) == '\x01') {
          FUN_00631590(*(undefined4 *)(iVar6 + 0x148),*(undefined4 *)(iVar6 + 0x14c),
                       *(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x209,1,0,0,0);
        }
        if (*(char *)(iVar7 + 0xe7) == '\x01') {
          FUN_00631590(*(undefined4 *)(iVar6 + 0x148),*(undefined4 *)(iVar6 + 0x14c),
                       *(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x208,1,0,0,0);
        }
      }
    }
  }
  fVar26 = DAT_006a4d04;
  iVar8 = *(int *)(iVar6 + 0x104);
  if (iVar8 < 0x87) {
    if (iVar8 != 0x86) {
      if (iVar8 < 0x24) {
        if ((iVar8 != 0x23) && (iVar8 != 4)) {
          bVar25 = iVar8 == 0xd;
          goto LAB_006a4942;
        }
      }
      else if ((iVar8 < 0x7d) || (0x7f < iVar8)) goto LAB_006a4944;
    }
LAB_006a4962:
    local_9c = *(undefined4 *)(iVar7 + 0x100);
    local_94 = *(undefined4 *)(iVar7 + 0x108);
    local_98 = *(int *)(iVar7 + 0x104) + -0x20;
    local_90 = *(undefined4 *)(iVar7 + 0x10c);
  }
  else {
    if ((iVar8 == 0xde) || (iVar8 == 0x106)) goto LAB_006a4962;
    bVar25 = iVar8 == 0x3fc;
LAB_006a4942:
    if (bVar25) goto LAB_006a4962;
LAB_006a4944:
    local_9c = *(undefined4 *)(iVar6 + 0x148);
    local_98 = *(int *)(iVar6 + 0x14c);
    local_94 = *(undefined4 *)(iVar6 + 0x150);
    local_90 = *(undefined4 *)(iVar6 + 0x154);
  }
  uVar15 = DAT_00fd67f4;
  uVar10 = DAT_00fd6800;
  if (*(int *)(iVar6 + 0x104) == 0x16) {
    FUN_007aa89c(&DAT_00fe23d0,"Harald");
    FUN_007aa89c(&DAT_00fe23d0,"Niels");
    FUN_007aa89c(&DAT_00fe23d0,"Andrew");
    uVar15 = DAT_00fd67f8;
    uVar10 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar10 = uVar10 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    DAT_00fd67fc = DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar10 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * DAT_006a4d00) == 0) {
      DAT_00fd6800 = uVar10;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x363,1,0,0,0);
      uVar15 = DAT_00fd67f4;
      uVar10 = DAT_00fd6800;
    }
  }
  iVar7 = local_98;
  uVar17 = local_9c;
  fVar27 = DAT_006a4cfc;
  uVar15 = uVar15 ^ uVar15 << 0xb;
  DAT_00fd67f4 = DAT_00fd67f8;
  DAT_00fd67f8 = DAT_00fd67fc;
  DAT_00fd6800 = uVar15 ^ (uVar15 ^ uVar10 >> 0xb) >> 8 ^ uVar10;
  fVar28 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
  DAT_00fd67fc = uVar10;
  if (((int)(fVar28 * fVar26 * DAT_006a4cfc) == 0) &&
     (iVar8 = FUN_00654790(*(undefined4 *)(iVar6 + 0x104)), 0 < iVar8)) {
    FUN_00631590(uVar17,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),iVar8,1,
                 0,0,0);
    iVar7 = local_98;
    uVar17 = local_9c;
  }
  if (DAT_010338d3 != '\0') {
    fVar28 = *(float *)(iVar6 + 0x1ec);
    uVar15 = in_fpscr & 0xfffffff | (uint)(fVar28 < 0.0) << 0x1f | (uint)(fVar28 == 0.0) << 0x1e;
    in_fpscr = uVar15 | (uint)NAN(fVar28) << 0x1c;
    bVar4 = (byte)(uVar15 >> 0x18);
    if (!(bool)(bVar4 >> 6 & 1) && bVar4 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
      if ((DAT_00fe1f88 == '\0') && (iVar8 = FUN_00608330(&DAT_00fd67f4,2000), iVar8 == 0)) {
        FUN_00631590(uVar17,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x22c,1,0,0,0);
      }
      else if ((DAT_00fe1f89 == '\0') && (iVar8 = FUN_00608330(&DAT_00fd67f4,2000), iVar8 == 0)) {
        FUN_00631590(uVar17,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x220,1,0,0,0);
      }
      else if ((DAT_00fe1f8a == '\0') && (iVar8 = FUN_00608330(&DAT_00fd67f4,2000), iVar8 == 0)) {
        FUN_00631590(uVar17,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x22d,1,0,0,0);
      }
    }
  }
  iVar7 = FUN_007354a8();
  if (iVar7 != 0) {
    fVar28 = *(float *)(iVar6 + 0x1ec);
    uVar15 = in_fpscr & 0xfffffff | (uint)(fVar28 < 0.0) << 0x1f | (uint)(fVar28 == 0.0) << 0x1e;
    in_fpscr = uVar15 | (uint)NAN(fVar28) << 0x1c;
    bVar4 = (byte)(uVar15 >> 0x18);
    if (((!(bool)(bVar4 >> 6 & 1) && bVar4 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) &&
        (*(int *)(iVar6 + 0x19c) < 0x28)) && (*(int *)(iVar6 + 0x1a0) < 0x14)) {
      iVar7 = FUN_00608330(&DAT_00fd67f4,0x6a4);
      if (iVar7 == 0) {
        uVar17 = 0x721;
      }
      else {
        iVar7 = FUN_00608330(&DAT_00fd67f4,700);
        if (iVar7 != 0) goto LAB_006a4c3a;
        uVar17 = 0x723;
      }
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   uVar17,1,0,0,0);
    }
  }
LAB_006a4c3a:
  uVar15 = DAT_00fd67f8;
  uVar10 = DAT_00fd67fc;
  uVar9 = DAT_00fd6800;
  if (DAT_010338d3 != '\0') {
    fVar28 = *(float *)(iVar6 + 0x1ec);
    uVar11 = in_fpscr & 0xfffffff | (uint)(fVar28 < 0.0) << 0x1f | (uint)(fVar28 == 0.0) << 0x1e;
    in_fpscr = uVar11 | (uint)NAN(fVar28) << 0x1c;
    bVar4 = (byte)(uVar11 >> 0x18);
    if (!(bool)(bVar4 >> 6 & 1) && bVar4 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
      iVar7 = FUN_006c314c(&local_9c);
      uVar9 = DAT_00fd67fc;
      uVar10 = DAT_00fd67f8;
      uVar15 = DAT_00fd67f4;
      fVar28 = DAT_006a4cf8;
      DAT_00fd67f4 = DAT_00fd67f8;
      uVar15 = uVar15 ^ uVar15 << 0xb;
      DAT_00fd67f8 = DAT_00fd67fc;
      uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
      DAT_00fd67fc = DAT_00fd6800;
      fVar29 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      uVar11 = DAT_00fd6800;
      if (((int)(fVar29 * fVar26 * DAT_006a4cf8) == 0) &&
         (uVar11 = DAT_00fd6800, *(char *)(iVar7 + 0xe9) != '\0')) {
        DAT_00fd6800 = uVar15;
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x70b,1,0,0,0);
        uVar10 = DAT_00fd67f4;
        uVar15 = DAT_00fd6800;
        uVar11 = DAT_00fd67fc;
        uVar9 = DAT_00fd67f8;
      }
      DAT_00fd67f4 = uVar9;
      DAT_00fd67f8 = uVar11;
      DAT_00fd67fc = uVar15;
      uVar10 = uVar10 ^ uVar10 << 0xb;
      DAT_00fd6800 = uVar10 ^ (uVar10 ^ DAT_00fd67fc >> 0xb) >> 8 ^ DAT_00fd67fc;
      fVar29 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      if (((int)(fVar29 * fVar26 * fVar28) == 0) && (*(char *)(iVar7 + 0xe6) != '\0')) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x70c,1,0,0,0);
      }
      uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
      uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
      fVar29 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      uVar10 = DAT_00fd67f8;
      if (((int)(fVar29 * fVar26 * fVar28) == 0) && (*(char *)(iVar7 + 0xeb) != '\0')) {
        DAT_00fd67f4 = DAT_00fd67f8;
        DAT_00fd67f8 = DAT_00fd67fc;
        DAT_00fd67fc = DAT_00fd6800;
        DAT_00fd6800 = uVar15;
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x70d,1,0,0,0);
        uVar15 = DAT_00fd6800;
        DAT_00fd6800 = DAT_00fd67fc;
        DAT_00fd67fc = DAT_00fd67f8;
        uVar10 = DAT_00fd67f4;
      }
      DAT_00fd67f4 = DAT_00fd67fc;
      DAT_00fd67f8 = DAT_00fd6800;
      DAT_00fd67fc = uVar15;
      uVar10 = uVar10 ^ uVar10 << 0xb;
      DAT_00fd6800 = uVar10 ^ (uVar10 ^ DAT_00fd67fc >> 0xb) >> 8 ^ DAT_00fd67fc;
      fVar29 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      if (((int)(fVar29 * fVar26 * fVar28) == 0) && (*(char *)(iVar7 + 0xe7) != '\0')) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x70e,1,0,0,0);
      }
      uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
      uVar9 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
      fVar29 = (float)VectorSignedToFloat(uVar9 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      DAT_00fd67f4 = DAT_00fd67f8;
      uVar15 = DAT_00fd67fc;
      uVar10 = DAT_00fd6800;
      if (((int)(fVar29 * fVar26 * fVar28) == 0) &&
         (uVar15 = DAT_00fd67fc, uVar10 = DAT_00fd6800, *(char *)(iVar7 + 0xea) != '\0')) {
        DAT_00fd67f8 = DAT_00fd67fc;
        DAT_00fd67fc = DAT_00fd6800;
        DAT_00fd6800 = uVar9;
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x70f,1,0,0,0);
        uVar15 = DAT_00fd67f8;
        uVar10 = DAT_00fd67fc;
        uVar9 = DAT_00fd6800;
      }
    }
  }
  DAT_00fd6800 = uVar9;
  DAT_00fd67fc = uVar10;
  DAT_00fd67f8 = uVar15;
  if (*(int *)(iVar6 + 0x104) == 0x44) {
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 0x491,1,0,0,0);
  }
  if (DAT_010703e3 != '\0') {
    FUN_006558d8(iVar6,&local_9c);
  }
  if (*(int *)(iVar6 + 0x104) == 0x145) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = uVar15;
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 0x6c1,0x1e - (int)(fVar28 * fVar26 * -21.0),0,0,0);
  }
  if (*(int *)(iVar6 + 0x104) == 0x146) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = uVar15;
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 0x6c1,1 - (int)(fVar28 * fVar26 * -4.0),0,0,0);
  }
  uVar15 = DAT_00fd6800;
  if ((0x130 < *(int *)(iVar6 + 0x104)) && (*(int *)(iVar6 + 0x104) < 0x13b)) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar28 * fVar26 * 4.0) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x3a,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  uVar15 = DAT_00fd6800;
  if (*(int *)(iVar6 + 0x104) == 0x146) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar28 * fVar26 * 6.0) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x3a,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  uVar15 = DAT_00fd6800;
  if (*(int *)(iVar6 + 0x104) == 0x149) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar28 * fVar26 * 4.0) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x3a,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  uVar15 = DAT_00fd6800;
  if (*(int *)(iVar6 + 0x104) == 0x14a) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar28 * fVar26 * 4.0) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x3a,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  if (*(int *)(iVar6 + 0x104) == 0x13b) {
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 0x3a,1,0,0,0);
  }
  if ((*(int *)(iVar6 + 0x104) == 0x145) || (*(int *)(iVar6 + 0x104) == 0x147)) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    iVar7 = 6 - (int)(fVar28 * fVar26 * -6.0);
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = uVar15;
    if (0 < iVar7) {
      do {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x3a,1,0,0,0);
        iVar7 = iVar7 + -1;
      } while (iVar7 != 0);
    }
  }
  uVar15 = DAT_00fd6800;
  if (*(int *)(iVar6 + 0x104) == 0x9c) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar28 * fVar26 * DAT_006a5508) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x5ee,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  uVar15 = DAT_00fd6800;
  if (*(int *)(iVar6 + 0x104) == 0xf3) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar28 * fVar26 * 3.0) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x5ef,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  uVar15 = DAT_00fd6800;
  if ((0x10c < *(int *)(iVar6 + 0x104)) && (*(int *)(iVar6 + 0x104) < 0x119)) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar28 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar28 * fVar26 * DAT_006a5504) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x5ed,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  fVar28 = DAT_006a5500;
  if ((*(int *)(iVar6 + 0x104) == 0x9e) || (uVar15 = DAT_00fd6800, *(int *)(iVar6 + 0x104) == 0x9f))
  {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar29 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar29 * fVar26 * DAT_006a5500) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x5f0,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  uVar15 = DAT_00fd6800;
  if (*(int *)(iVar6 + 0x104) == 0xb0) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar29 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar29 * fVar26 * fVar27) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x5f1,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  uVar15 = DAT_00fd6800;
  if (*(int *)(iVar6 + 0x104) == 0x30) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar27 * fVar26 * DAT_006a5918) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x5ec,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  uVar15 = DAT_00fd6800;
  if (*(int *)(iVar6 + 0x104) == 0xcd) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar27 * fVar26 * 2.0) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x64b,1,0,0,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  iVar7 = FUN_007354a8();
  if (iVar7 != 0) {
    if ((*(int *)(iVar6 + 0x104) == 0x79) || (*(int *)(iVar6 + 0x104) == 0x17)) {
      bVar25 = false;
    }
    else if (((*(int *)(iVar6 + 0x1b8) < 2) || (*(int *)(iVar6 + 0x19c) < 1)) ||
            (*(char *)(iVar6 + 0x11a) != '\0')) {
      bVar25 = false;
    }
    else {
      bVar25 = true;
    }
    if (bVar25) {
      fVar27 = *(float *)(iVar6 + 0x1ec);
      uVar15 = in_fpscr & 0xfffffff | (uint)(fVar27 < 0.0) << 0x1f | (uint)(fVar27 == 0.0) << 0x1e;
      in_fpscr = uVar15 | (uint)NAN(fVar27) << 0x1c;
      bVar4 = (byte)(uVar15 >> 0x18);
      if (!(bool)(bVar4 >> 6 & 1) && bVar4 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
        uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
        DAT_00fd67f4 = DAT_00fd67f8;
        DAT_00fd67f8 = DAT_00fd67fc;
        uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
        DAT_00fd67fc = DAT_00fd6800;
        fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
        DAT_00fd6800 = uVar15;
        if ((int)(fVar27 * fVar26 * DAT_006a5914) == 0) {
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),0x6ee,1,0,0,0);
        }
      }
    }
  }
  iVar7 = FUN_0073542c();
  uVar9 = DAT_00fd67fc;
  uVar10 = DAT_00fd67f8;
  uVar15 = DAT_00fd67f4;
  uVar11 = DAT_00fd6800;
  if (iVar7 != 0) {
    if ((*(int *)(iVar6 + 0x104) == 0x79) || (*(int *)(iVar6 + 0x104) == 0x17)) {
      bVar25 = false;
    }
    else if (((*(int *)(iVar6 + 0x1b8) < 2) || (*(int *)(iVar6 + 0x19c) < 1)) ||
            (*(char *)(iVar6 + 0x11a) != '\0')) {
      bVar25 = false;
    }
    else {
      bVar25 = true;
    }
    if (bVar25) {
      fVar27 = *(float *)(iVar6 + 0x1ec);
      uVar16 = in_fpscr & 0xfffffff | (uint)(fVar27 < 0.0) << 0x1f | (uint)(fVar27 == 0.0) << 0x1e;
      in_fpscr = uVar16 | (uint)NAN(fVar27) << 0x1c;
      bVar4 = (byte)(uVar16 >> 0x18);
      if (!(bool)(bVar4 >> 6 & 1) && bVar4 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
        DAT_00fd67f4 = DAT_00fd67f8;
        uVar15 = uVar15 ^ uVar15 << 0xb;
        DAT_00fd67f8 = DAT_00fd67fc;
        uVar11 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
        DAT_00fd67fc = DAT_00fd6800;
        fVar27 = (float)VectorSignedToFloat(uVar11 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
        if ((int)(fVar27 * fVar26 * 13.0) == 0) {
          uVar10 = uVar10 ^ uVar10 << 0xb;
          DAT_00fd67f4 = uVar9;
          uVar15 = uVar10 ^ (uVar10 ^ uVar11 >> 0xb) >> 8 ^ uVar11;
          DAT_00fd67f8 = DAT_00fd6800;
          fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
          DAT_00fd67fc = uVar11;
          DAT_00fd6800 = uVar15;
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),599 - (int)(fVar27 * fVar26 * -3.0),1,0,0,0);
          uVar11 = DAT_00fd6800;
        }
      }
    }
  }
  DAT_00fd6800 = uVar11;
  iVar7 = *(int *)(iVar6 + 0x104);
  if (((iVar7 == 0x3e) || (iVar7 == 0x3fb)) || (fVar27 = 1.0, iVar7 == 0x42)) {
    fVar27 = 8.0;
  }
  iVar7 = FUN_006c1308();
  if (*(char *)(iVar7 + 0x5d23) == '\x1e') {
    fVar27 = fVar27 * 5.0;
  }
  iVar7 = FUN_00735468();
  fVar29 = DAT_006a5910;
  if (iVar7 == 1) {
    if ((*(int *)(iVar6 + 0x104) == 0x79) || (*(int *)(iVar6 + 0x104) == 0x17)) {
      bVar25 = false;
    }
    else if (((*(int *)(iVar6 + 0x1b8) < 2) || (*(int *)(iVar6 + 0x19c) < 1)) ||
            (*(char *)(iVar6 + 0x11a) != '\0')) {
      bVar25 = false;
    }
    else {
      bVar25 = true;
    }
    if (bVar25) {
      uVar15 = DAT_01070468 ^ DAT_01070468 << 0xb;
      DAT_01070468 = DAT_0107046c;
      DAT_0107046c = DAT_01070470;
      uVar15 = uVar15 ^ (uVar15 ^ DAT_01070474 >> 0xb) >> 8 ^ DAT_01070474;
      DAT_01070470 = DAT_01070474;
      fVar30 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      fVar30 = (float)VectorSignedToFloat((int)(fVar30 * fVar26 * DAT_006a5910),
                                          (byte)(in_fpscr >> 0x16) & 3);
      in_fpscr = in_fpscr & 0xfffffff | (uint)(fVar27 <= fVar30) << 0x1d;
      DAT_01070474 = uVar15;
      if ((byte)(in_fpscr >> 0x1d) == 0) {
        FUN_00631590(*(undefined4 *)(iVar6 + 0x148),*(undefined4 *)(iVar6 + 0x14c),
                     *(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x13c8,1,0,0,0);
      }
    }
  }
  if ((*(int *)(iVar6 + 0x104) == 0x119) || (*(int *)(iVar6 + 0x104) == 0x11a)) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    DAT_00fd67fc = DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd6800 = uVar15;
    if ((int)(fVar27 * fVar26 * fVar28) == 0) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x5a6,1,0,0xffffffff,0);
    }
  }
  if ((*(int *)(iVar6 + 0x104) == 0x11b) ||
     (uVar15 = DAT_00fd6800, *(int *)(iVar6 + 0x104) == 0x11c)) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar27 * fVar26 * fVar28) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x5a4,1,0,0xffffffff,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  if ((*(int *)(iVar6 + 0x104) == 0x11d) ||
     (uVar15 = DAT_00fd6800, *(int *)(iVar6 + 0x104) == 0x11e)) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar27 * fVar26 * fVar28) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x5a5,1,0,0xffffffff,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  if (*(int *)(iVar6 + 0x104) == 0x120) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = uVar15;
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 0x5e4,2 - (int)(fVar27 * fVar26 * -2.0),0,0xffffffff,0);
  }
  else if ((((DAT_010338d3 != '\0') && (DAT_00fe1f8d != '\0')) && (*(byte *)(iVar6 + 0x175) != 4))
          && ((*(char *)((&DAT_0107034c)[*(byte *)(iVar6 + 0x175)] + 0xe5) != '\0' &&
              (iVar7 = FUN_00608330(&DAT_00fd67f4,0xc), iVar7 == 0)))) {
    FUN_00685550(*(int *)(iVar6 + 0x148) + (*(int *)(iVar6 + 0x150) >> 1),
                 *(int *)(iVar6 + 0x14c) + (*(int *)(iVar6 + 0x154) >> 1),0x120,0);
  }
  if ((*(int *)(iVar6 + 0x104) == 0xa2) || (uVar15 = DAT_00fd6800, *(int *)(iVar6 + 0x104) == 0xa6))
  {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar27 * fVar26 * DAT_006a5d90) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x622,1,0,0xffffffff,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  if ((*(int *)(iVar6 + 0x104) == 0x9e) || (uVar15 = DAT_00fd6800, *(int *)(iVar6 + 0x104) == 0x9f))
  {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    if ((int)(fVar27 * fVar26 * fVar29) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   900,1,0,0xffffffff,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  if (*(int *)(iVar6 + 0x104) == 0x23) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = uVar15;
    if ((int)(fVar27 * fVar26 * 7.0) == 0) {
      uVar17 = 0x501;
    }
    else {
      iVar7 = FUN_00608330(&DAT_00fd67f4,7);
      if (iVar7 == 0) {
        uVar17 = 0x4f9;
      }
      else {
        iVar7 = FUN_00608330(&DAT_00fd67f4,7);
        if (iVar7 != 0) goto LAB_006a5de8;
        uVar17 = 0x521;
      }
    }
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 uVar17,1,0,0xffffffff,0);
  }
LAB_006a5de8:
  if (((DAT_010703e2 != '\0') && (DAT_010338d3 != '\0')) &&
     (iVar7 = FUN_00608330(&DAT_00fd67f4,1000), iVar7 == 0)) {
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 0x522,1,0,0xffffffff,0);
  }
  uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
  DAT_00fd67f4 = DAT_00fd67f8;
  DAT_00fd67f8 = DAT_00fd67fc;
  uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
  fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
  DAT_00fd67fc = DAT_00fd6800;
  if ((int)(fVar27 * fVar26 * fVar29) == 0) {
    iVar7 = *(int *)(iVar6 + 0x104);
    if (((iVar7 == 0x68) || (iVar7 == 0x66)) || ((0x10c < iVar7 && (iVar7 < 0x111)))) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x375,1,0,0xffffffff,0);
      uVar15 = DAT_00fd6800;
    }
    DAT_00fd6800 = uVar15;
    iVar7 = *(int *)(iVar6 + 0x104);
    if ((iVar7 == 0x4d) || ((0x110 < iVar7 && (iVar7 < 0x115)))) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x376,1,0,0xffffffff,0);
    }
    iVar7 = *(int *)(iVar6 + 0x104);
    if (((((iVar7 == 0x8d) || (iVar7 == 0xb2)) || (iVar7 == 0x2a)) ||
        ((0xe6 < iVar7 && (iVar7 < 0xec)))) || (iVar7 == 0x3f3)) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x377,1,0,0xffffffff,0);
    }
    iVar7 = *(int *)(iVar6 + 0x104);
    if (((iVar7 == 0x4f) || (iVar7 == 0x51)) || (iVar7 == 0x3ec)) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x378,1,0,0xffffffff,0);
    }
    iVar7 = *(int *)(iVar6 + 0x104);
    if (((iVar7 == 0x4b) || (iVar7 == 0x4e)) || (iVar7 == 0x52)) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x379,1,0,0xffffffff,0);
    }
    iVar7 = *(int *)(iVar6 + 0x104);
    if (((iVar7 == 0x4b) || (iVar7 == 0x4f)) || ((iVar7 == 0x67 || (iVar7 == 0x3ee)))) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x37a,1,0,0xffffffff,0);
    }
    iVar7 = *(int *)(iVar6 + 0x104);
    if (((iVar7 == 0x22) || (iVar7 == 0x3f4)) ||
       ((iVar7 == 0x53 || ((iVar7 == 0x54 || (iVar7 == 0x3ed)))))) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x37b,1,0,0xffffffff,0);
    }
    if ((*(int *)(iVar6 + 0x104) == 0x5e) || (*(int *)(iVar6 + 0x104) == 0xb6)) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x37c,1,0,0xffffffff,0);
    }
    iVar7 = *(int *)(iVar6 + 0x104);
    if ((((iVar7 == 0x5d) || (iVar7 == 0x6d)) || (iVar7 == 0x50)) ||
       (uVar15 = DAT_00fd6800, iVar7 == 0x3f1)) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x37d,1,0,0xffffffff,0);
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  if (*(int *)(iVar6 + 0x104) == 0x122) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * DAT_006a6150) == 0) {
      uVar17 = 0x5e9;
    }
    else {
      iVar7 = FUN_00608330(&DAT_00fd67f4,0xf);
      if (iVar7 != 0) goto LAB_006a618a;
      uVar17 = 0x3aa;
    }
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 uVar17,1,0,0xffffffff,0);
  }
LAB_006a618a:
  if (*(int *)(iVar6 + 0x104) == 0x123) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * 15.0) == 0) {
      uVar17 = 0x514;
    }
    else {
      iVar7 = FUN_00608330(&DAT_00fd67f4,0xf);
      if (iVar7 != 0) goto LAB_006a6236;
      uVar17 = 0x4e6;
    }
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 uVar17,1,0,0xffffffff,0);
  }
LAB_006a6236:
  if (*(int *)(iVar6 + 0x104) == 0x124) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * 15.0) == 0) {
      uVar17 = 0x5ea;
    }
    else {
      iVar7 = FUN_00608330(&DAT_00fd67f4,0xf);
      if (iVar7 != 0) goto LAB_006a62dc;
      uVar17 = 0x2a7;
    }
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 uVar17,1,0,0xffffffff,0);
  }
LAB_006a62dc:
  if (*(int *)(iVar6 + 0x104) == 0x125) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * 20.0) == 0) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x2f7,1,0,0xffffffff,0);
    }
  }
  if (*(int *)(iVar6 + 0x104) == 0x9c) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * 30.0) == 0) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x2ab,1,0,0xffffffff,0);
    }
  }
  if (*(int *)(iVar6 + 0x104) == 0xf5) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    iVar7 = (int)(fVar27 * fVar26 * 8.0);
    if (iVar7 == 0) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x4ea,1,0,0xffffffff,0);
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x3c,100);
      uVar31 = 0;
      uVar18 = 0x4ed;
LAB_006a6528:
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   uVar18,uVar17,0,uVar31,0);
    }
    else {
      if (iVar7 == 1) {
        uVar31 = 0xffffffff;
        uVar17 = 1;
        uVar18 = 0x462;
        goto LAB_006a6528;
      }
      if (iVar7 == 2) {
        uVar31 = 0xffffffff;
        uVar17 = 1;
        uVar18 = 899;
        goto LAB_006a6528;
      }
      if (iVar7 == 3) {
        uVar31 = 0xffffffff;
        uVar17 = 1;
        uVar18 = 0x4e0;
        goto LAB_006a6528;
      }
      if (iVar7 == 4) {
        uVar31 = 0xffffffff;
        uVar17 = 1;
        uVar18 = 0x50e;
        goto LAB_006a6528;
      }
      if (iVar7 == 5) {
        uVar31 = 0xffffffff;
        uVar17 = 1;
        uVar18 = 0x50f;
        goto LAB_006a6528;
      }
      if (iVar7 == 6) {
        uVar31 = 0xffffffff;
        uVar17 = 1;
        uVar18 = 0x510;
        goto LAB_006a6528;
      }
      if (iVar7 == 7) {
        uVar31 = 0xffffffff;
        uVar17 = 1;
        uVar18 = 0x511;
        goto LAB_006a6528;
      }
    }
    DAT_00fe1f8e = 1;
  }
  if (*(int *)(iVar6 + 0x104) == 0x10c) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 0x534,2 - (int)(fVar27 * fVar26 * -2.0),0,0,0);
  }
  if (*(int *)(iVar6 + 0x104) == 0x99) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * 17.0) != 0) goto LAB_006a664c;
    uVar17 = 0x530;
LAB_006a6706:
    uVar14 = *(undefined2 *)(iVar6 + 0x168);
LAB_006a6712:
    FUN_00631590(local_9c,local_98,uVar14,*(undefined2 *)(iVar6 + 0x16a),uVar17,1,0,0xffffffff,0);
  }
  else {
LAB_006a664c:
    if ((*(int *)(iVar6 + 0x104) == 0xfd) && (iVar7 = FUN_00608330(&DAT_00fd67f4,0xfa), iVar7 == 0))
    {
      uVar17 = 0x52f;
      goto LAB_006a6706;
    }
    if ((*(int *)(iVar6 + 0x104) == 0x78) && (iVar7 = FUN_00608330(&DAT_00fd67f4,500), iVar7 == 0))
    {
      uVar17 = 0x52e;
      goto LAB_006a6706;
    }
    if ((*(int *)(iVar6 + 0x104) == 0x31) && (iVar7 = FUN_00608330(&DAT_00fd67f4,0xfa), iVar7 == 0))
    {
      uVar17 = 0x52d;
      goto LAB_006a6706;
    }
    if ((*(int *)(iVar6 + 0x104) == 0xb9) && (iVar7 = FUN_00608330(&DAT_00fd67f4,0x96), iVar7 == 0))
    {
      uVar17 = 0x3b7;
      goto LAB_006a6706;
    }
    if ((*(int *)(iVar6 + 0x104) == 0x2c) && (iVar7 = FUN_00608330(&DAT_00fd67f4,0x4b), iVar7 == 0))
    {
      uVar17 = 0x528;
      goto LAB_006a6706;
    }
    if ((*(int *)(iVar6 + 0x104) == 0x6e) && (iVar7 = FUN_00608330(&DAT_00fd67f4,100), iVar7 == 0))
    {
      uVar17 = 0x529;
      goto LAB_006a6706;
    }
    if (((*(int *)(iVar6 + 0x104) == 0x3c) && (iVar7 = FUN_00608330(&DAT_00fd67f4,0x96), iVar7 == 0)
        ) || ((*(int *)(iVar6 + 0x104) == 0x97 &&
              (iVar7 = FUN_00608330(&DAT_00fd67f4,0x32), iVar7 == 0)))) {
      uVar17 = 0x52a;
      goto LAB_006a6706;
    }
    if ((*(int *)(iVar6 + 0x104) != 0x18) || (iVar7 = FUN_00608330(&DAT_00fd67f4,0x4b), iVar7 != 0))
    {
      if ((*(int *)(iVar6 + 0x104) == 0x6d) &&
         (iVar7 = FUN_00608330(&DAT_00fd67f4,0x1e), iVar7 == 0)) {
        uVar17 = FUN_005c4e6c(&DAT_00fd67f4,1,5);
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x52c,uVar17,0,0xffffffff,0);
        goto LAB_006a7160;
      }
      iVar7 = *(int *)(iVar6 + 0x104);
      if ((iVar7 == 0xa3) || (iVar7 == 0xee)) {
        iVar7 = FUN_00608330(&DAT_00fd67f4,0x28);
        if (iVar7 == 0) {
          uVar17 = 0x51c;
          goto LAB_006a6706;
        }
        goto LAB_006a7160;
      }
      if (iVar7 != 0x11f) {
        if (iVar7 == 0x4d) {
          iVar7 = FUN_00608330(&DAT_00fd67f4,0x96);
          if (iVar7 == 0) {
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x2d3,1,0,0xffffffff,0);
          }
        }
        else if (iVar7 == 0xfb) {
          iVar7 = FUN_00608330(&DAT_00fd67f4,0x32);
          if (iVar7 == 0) {
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x51f,1,0,0xffffffff,0);
          }
        }
        else if (iVar7 == 0xc5) {
          iVar7 = FUN_00608330(&DAT_00fd67f4,200);
          if (iVar7 == 0) {
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x51a,1,0,0xffffffff,0);
          }
        }
        else if (iVar7 == 0xf4) {
          uVar17 = FUN_005c4e6c(&DAT_00fd67f4,1,6);
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),0x17,uVar17,0,0,0);
          iVar7 = FUN_00608330(&DAT_00fd67f4,2);
          if (iVar7 == 0) {
            uVar17 = FUN_005c4e6c(&DAT_00fd67f4,1,6);
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x17,uVar17,0,0,0);
          }
          iVar7 = FUN_00608330(&DAT_00fd67f4,3);
          if (iVar7 == 0) {
            uVar17 = FUN_005c4e6c(&DAT_00fd67f4,3,9);
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x17,uVar17,0,0,0);
          }
          iVar7 = FUN_00608330(&DAT_00fd67f4,4);
          if (iVar7 == 0) {
            uVar17 = FUN_005c4e6c(&DAT_00fd67f4,6,0xc);
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x17,uVar17,0,0,0);
          }
          uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x1e,0x3c);
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),0x296,uVar17,0,0,0);
        }
        else if (iVar7 == 0xfa) {
          iVar7 = FUN_00608330(&DAT_00fd67f4,0xf);
          if (iVar7 == 0) {
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x4dc,1,0,0xffffffff,0);
          }
        }
        else if (iVar7 == 0xac) {
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),0x2f2,1,0,0xffffffff,0);
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),0x2f3,1,0,0xffffffff,0);
        }
        else if (iVar7 == 0x6e) {
          iVar7 = FUN_00608330(&DAT_00fd67f4,200);
          if (iVar7 == 0) {
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x2aa,1,0,0xffffffff,0);
          }
        }
        else {
          if ((0x10c < iVar7) && (iVar7 < 0x119)) {
            iVar7 = FUN_00608330(&DAT_00fd67f4,600);
            if (iVar7 == 0) {
              uVar17 = 0x49f;
            }
            else {
              iVar7 = FUN_00608330(&DAT_00fd67f4,400);
              if (iVar7 != 0) {
                iVar7 = FUN_00608330(&DAT_00fd67f4,300);
                if (iVar7 == 0) {
                  FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                               *(undefined2 *)(iVar6 + 0x16a),0x29f,1,0,0xffffffff,0);
                }
                goto LAB_006a7160;
              }
              uVar17 = 0x4f2;
            }
            goto LAB_006a6706;
          }
          if (iVar7 == 0x9a) {
            iVar7 = FUN_00608330(&DAT_00fd67f4,100);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x4e5,1,0,0xffffffff,0);
            }
          }
          else if (((iVar7 == 0xa9) || (iVar7 == 0xce)) || (iVar7 == 0x96)) {
            iVar7 = FUN_00608330(&DAT_00fd67f4,0x32);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x2d6,1,0,0xffffffff,0);
            }
          }
          else if (iVar7 == 0xf3) {
            uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x2ac,0x2af);
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),uVar17,1,0,0xffffffff,0);
          }
          else if (((iVar7 == 0xc6) || (iVar7 == 199)) || (iVar7 == 0xe2)) {
            iVar7 = FUN_00608330(&DAT_00fd67f4,3000);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x494,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,100);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x50d,1,0,0xffffffff,0);
            }
          }
          else if (((iVar7 == 0x4e) || (iVar7 == 0x4f)) ||
                  ((iVar7 == 0x50 || ((iVar7 == 0x3ee || (iVar7 == 0x3f1)))))) {
            iVar7 = FUN_00608330(&DAT_00fd67f4,0x4b);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x366,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,0x4b);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x367,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,0x4b);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x368,1,0,0xffffffff,0);
            }
          }
          else if ((iVar7 < 0xd4) || (0xd7 < iVar7)) {
            if (iVar7 == 0xd8) {
              iVar7 = FUN_00608330(&DAT_00fd67f4,4000);
              if (iVar7 == 0) {
                FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                             *(undefined2 *)(iVar6 + 0x16a),0x389,1,0,0xffffffff,0);
              }
              iVar7 = FUN_00608330(&DAT_00fd67f4,2000);
              if (iVar7 == 0) {
                FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                             *(undefined2 *)(iVar6 + 0x16a),0x357,1,0,0xffffffff,0);
              }
              iVar7 = FUN_00608330(&DAT_00fd67f4,1000);
              if (iVar7 == 0) {
                FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                             *(undefined2 *)(iVar6 + 0x16a),0x356,1,0,0xffffffff,0);
              }
              iVar7 = FUN_00608330(&DAT_00fd67f4,100);
              if (iVar7 == 0) {
                FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                             *(undefined2 *)(iVar6 + 0x16a),0x2a0,1,0,0xffffffff,0);
              }
            }
          }
          else {
            iVar7 = FUN_00608330(&DAT_00fd67f4,8000);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x389,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,4000);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x357,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,2000);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x356,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,200);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x2a0,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,500);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x4fd,1,0,0,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,500);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x4fe,1,0,0,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,500);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x4ff,1,0,0,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,500);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x500,1,0,0,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,300);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x6a8,1,0,0,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,300);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x6a9,1,0,0,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,300);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x6ae,1,0,0,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,300);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x6b4,1,0,0,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,300);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x6b8,1,0,0,0);
            }
          }
        }
        goto LAB_006a7160;
      }
      iVar7 = FUN_00608330(&DAT_00fd67f4,7);
      if (iVar7 != 0) goto LAB_006a7160;
      iVar7 = FUN_00608330(&DAT_00fd67f4,2);
      uVar14 = *(undefined2 *)(iVar6 + 0x168);
      if (iVar7 != 0) {
        FUN_00631590(local_9c,local_98,uVar14,*(undefined2 *)(iVar6 + 0x16a),0x3d1,1,0,0xffffffff,0)
        ;
        goto LAB_006a7160;
      }
      uVar17 = 0x3c3;
      goto LAB_006a6712;
    }
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 0x52b,1,0,0xffffffff,0);
  }
LAB_006a7160:
  if (*(int *)(iVar6 + 0x104) == 0xa1) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * DAT_006a7204) == 0) {
      iVar7 = FUN_00608330(&DAT_00fd67f4,3);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   iVar7 + 0x323,1,0,0,0);
    }
  }
  iVar7 = *(int *)(iVar6 + 0x104);
  if (iVar7 == 0xd9) {
    uVar17 = 0x45b;
LAB_006a7400:
    uVar18 = 0xffffffff;
LAB_006a7402:
    uVar14 = *(undefined2 *)(iVar6 + 0x168);
LAB_006a7418:
    FUN_00631590(local_9c,local_98,uVar14,*(undefined2 *)(iVar6 + 0x16a),uVar17,1,0,uVar18,0);
  }
  else {
    if (iVar7 == 0xda) {
      uVar17 = 0x45c;
      goto LAB_006a7400;
    }
    if (iVar7 == 0xdb) {
      uVar17 = 0x45d;
      goto LAB_006a7400;
    }
    if (iVar7 == 0xdc) {
      uVar17 = 0x45e;
      goto LAB_006a7400;
    }
    if (iVar7 == 0xdd) {
      uVar17 = 0x45f;
      goto LAB_006a7400;
    }
    if (iVar7 == 0xa7) {
      iVar7 = FUN_00608330(&DAT_00fd67f4,0x32);
      if (iVar7 != 0) goto LAB_006a7422;
      uVar17 = 0x36f;
      goto LAB_006a7400;
    }
    if (iVar7 == 0x31) {
      iVar7 = FUN_00608330(&DAT_00fd67f4,200);
      if (iVar7 != 0) goto LAB_006a7422;
      uVar17 = 0x12;
      goto LAB_006a7400;
    }
    if ((((iVar7 == 0x15) || (iVar7 == 0xc9)) || (iVar7 == 0xca)) ||
       (((iVar7 == 0xcb || (iVar7 == 0x142)) || ((iVar7 == 0x143 || (iVar7 == 0x144)))))) {
      iVar7 = FUN_00608330(&DAT_00fd67f4,100);
      if (iVar7 == 0) {
        uVar17 = 0x3ba;
      }
      else {
        iVar7 = FUN_00608330(&DAT_00fd67f4,200);
        if (iVar7 == 0) {
          uVar17 = 0x3bb;
        }
        else {
          iVar7 = FUN_00608330(&DAT_00fd67f4,200);
          if (iVar7 == 0) {
            uVar17 = 0x48e;
          }
          else {
            iVar7 = FUN_00608330(&DAT_00fd67f4,500);
            if (iVar7 != 0) goto LAB_006a7422;
            uVar17 = 0x4fa;
          }
        }
      }
      goto LAB_006a7400;
    }
    if (iVar7 == 6) {
      iVar7 = FUN_00608330(&DAT_00fd67f4,0x96);
      if (iVar7 != 0) goto LAB_006a7422;
      iVar7 = FUN_00608330(&DAT_00fd67f4,3);
      if (iVar7 == 0) {
        uVar17 = 0x3bc;
        goto LAB_006a7400;
      }
      uVar14 = *(undefined2 *)(iVar6 + 0x168);
      uVar18 = 0xffffffff;
      if (iVar7 == 1) {
        uVar17 = 0x3bd;
      }
      else {
        uVar17 = 0x3be;
      }
      goto LAB_006a7418;
    }
    if (((iVar7 != 0x2a) && (iVar7 != 0x2b)) && ((iVar7 < 0xe7 || (0xeb < iVar7)))) {
      if ((((iVar7 == 0x1f) || (iVar7 == 0x20)) || (iVar7 == 0x126)) ||
         ((iVar7 == 0x127 || (iVar7 == 0x128)))) {
        iVar7 = FUN_00608330(&DAT_00fd67f4,0x1c2);
        if (iVar7 == 0) {
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),0x3bf,1,0,0xffffffff,0);
        }
        iVar7 = FUN_00608330(&DAT_00fd67f4,300);
        if (iVar7 == 0) {
          uVar17 = 0x51b;
          goto LAB_006a7400;
        }
        goto LAB_006a7422;
      }
      if ((((iVar7 != 0xae) && (iVar7 != 0xb3)) && ((iVar7 != 0xb6 && (iVar7 != 0xb7)))) ||
         (iVar7 = FUN_00608330(&DAT_00fd67f4,200), iVar7 != 0)) goto LAB_006a7422;
      uVar18 = 0;
      uVar17 = 0x3e4;
      goto LAB_006a7402;
    }
    iVar7 = FUN_00608330(&DAT_00fd67f4,100);
    if (iVar7 == 0) {
      iVar7 = FUN_00608330(&DAT_00fd67f4,3);
      if (iVar7 == 0) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x3c0,1,0,0xffffffff,0);
      }
      else if (iVar7 == 1) {
        uVar17 = 0x3c1;
        goto LAB_006a7400;
      }
      uVar17 = 0x3c2;
      goto LAB_006a7400;
    }
  }
LAB_006a7422:
  if (*(int *)(iVar6 + 0x104) == 0xba) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 0x28,1 - (int)(fVar27 * fVar26 * -9.0),0,0,0);
  }
  if (*(int *)(iVar6 + 0x104) == 0xe1) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * DAT_006a7518) == 0) {
      uVar17 = 1;
      uVar18 = 0x4db;
    }
    else {
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,2,6);
      uVar18 = 0x17;
    }
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 uVar18,uVar17,0,0,0);
  }
  if (*(int *)(iVar6 + 0x104) == 0xdf) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * 10.0) == 0) {
      iVar7 = FUN_00608330(&DAT_00fd67f4,2);
      if (iVar7 == 0) {
        uVar17 = 0x46f;
      }
      else {
        uVar17 = 0x470;
      }
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   uVar17,1,0,0xffffffff,0);
    }
  }
  iVar7 = local_98;
  fVar27 = *(float *)(iVar6 + 0x1ec);
  uVar15 = in_fpscr & 0xfffffff | (uint)(fVar27 < 0.0) << 0x1f | (uint)(fVar27 == 0.0) << 0x1e;
  uVar10 = uVar15 | (uint)NAN(fVar27) << 0x1c;
  bVar4 = (byte)(uVar15 >> 0x18);
  if (((!(bool)(bVar4 >> 6 & 1) && bVar4 >> 7 == ((byte)(uVar10 >> 0x1c) & 1)) &&
      (DAT_010338d3 != '\0')) && (local_98 < DAT_01050304 + 0xa0)) {
    iVar19 = *(int *)(iVar6 + 0x148) + (*(int *)(iVar6 + 0x150) >> 1);
    iVar8 = (DAT_0087e6f4 + 0x1e) * 0x10;
    if (((iVar19 < iVar8) || (iVar8 < iVar19)) &&
       (iVar8 = FUN_00608330(&DAT_00fd67f4,100), iVar8 == 0)) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x523,1,0,0,0);
      iVar7 = local_98;
    }
  }
  if (*(int *)(iVar6 + 0x104) == 0xde) {
    DAT_00fe1f8c = 1;
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    iVar8 = (int)(fVar27 * fVar26 * 3.0);
    if (iVar8 == 0) {
      iVar8 = 0x461;
    }
    else if (iVar8 == 1) {
      iVar8 = 0x463;
    }
    else if (iVar8 == 2) {
      iVar8 = 0x46c;
    }
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),iVar8,
                 1,0,0,0);
    uVar11 = DAT_00fd67fc;
    uVar9 = DAT_00fd67f8;
    uVar15 = DAT_00fd67f4;
    DAT_00fd67f4 = DAT_00fd67f8;
    uVar15 = uVar15 ^ uVar15 << 0xb;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    DAT_00fd67fc = DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * 20.0) == 0) {
      DAT_00fd6800 = uVar15;
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x492,1,0,0,0);
      uVar15 = DAT_00fd6800;
      uVar9 = DAT_00fd67f4;
      uVar11 = DAT_00fd67f8;
    }
    uVar9 = uVar9 ^ uVar9 << 0xb;
    DAT_00fd6800 = uVar15 ^ (uVar9 ^ uVar15 >> 0xb) >> 8 ^ uVar9;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    DAT_00fd67f4 = uVar11;
    DAT_00fd67f8 = DAT_00fd67fc;
    if ((int)(fVar27 * fVar26 * 3.0) == 0) {
      uVar17 = 0x469;
      DAT_00fd67fc = uVar15;
LAB_006a77fa:
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   uVar17,1,0,0,0);
    }
    else {
      DAT_00fd67fc = uVar15;
      iVar7 = FUN_00608330(&DAT_00fd67f4,2);
      if (iVar7 == 0) {
        uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x34a,0x34d);
        goto LAB_006a77fa;
      }
    }
    iVar8 = FUN_00608330(&DAT_00fd67f4,4);
    iVar7 = local_98;
    if (iVar8 != 0) {
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,10,0x1e);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x46a,uVar17,0,0,0);
      iVar7 = local_98;
    }
  }
  iVar8 = 0x10b;
  if (*(int *)(iVar6 + 0x104) == 0x10a) {
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0xf,0x29);
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x370,
                 uVar17,0,0,0);
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0xf,0x29);
    uVar18 = 0x370;
LAB_006a7962:
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 uVar18,uVar17,0,0,0);
    iVar7 = local_98;
  }
  else if ((*(int *)(iVar6 + 0x104) == 0x10b) && (iVar19 = FUN_00656370(0x10a), iVar19 != 0)) {
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,2,6);
    iVar19 = FUN_00608330(&DAT_00fd67f4,3);
    if (iVar19 != 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x531,uVar17,0,0,0);
      iVar7 = local_98;
    }
    iVar19 = FUN_00608330(&DAT_00fd67f4,3);
    if (iVar19 != 0) {
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,4,0xb);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x370,uVar17,0,0,0);
      iVar7 = local_98;
    }
    iVar19 = FUN_00608330(&DAT_00fd67f4,2);
    if (iVar19 == 0) {
      FUN_006c314c(&local_9c);
      iVar19 = FUN_006be868();
      iVar7 = local_98;
      if (iVar19 != 0) {
        uVar17 = 1;
        uVar18 = 0x3a;
        goto LAB_006a7962;
      }
    }
  }
  iVar19 = *(int *)(iVar6 + 0x104);
  if (0x93 < iVar19) {
    if (iVar19 < 0x13f) {
      if (0x13c < iVar19) {
switchD_006a79a2_caseD_2:
        iVar8 = FUN_00608330(&DAT_00fd67f4,0x96);
        if (iVar8 < 100) {
          if (iVar8 == 0x26) {
            uVar17 = 0xec;
          }
          else {
            uVar17 = 0x26;
          }
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       uVar17,1,0,0,0);
          iVar7 = local_98;
          goto LAB_006a99ce;
        }
        goto switchD_006a79a2_caseD_5;
      }
      if (200 < iVar19) {
        if (iVar19 < 0xec) {
          if (0xe6 < iVar19) goto switchD_006a79a2_caseD_2a;
          if (iVar19 < 0xe0) {
            if (iVar19 == 0xdf) goto switchD_006a79a2_caseD_3;
            if (200 < iVar19) {
              if (iVar19 < 0xcc) goto switchD_006a79a2_caseD_15;
              if (iVar19 == 0xcc) goto switchD_006a79a2_caseD_1;
            }
          }
          else if (iVar19 == 0xe6) goto switchD_006a79a2_caseD_37;
        }
        else if (iVar19 < 0x107) {
          if (iVar19 == 0x106) {
            FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x475,1,0,0xffffffff,0);
            iVar7 = FUN_00608330(&DAT_00fd67f4,0x14);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x49e,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,200);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x519,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,4);
            if (iVar7 == 0) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x485,1,0,0xffffffff,0);
            }
            iVar7 = FUN_00608330(&DAT_00fd67f4,6);
            if ((DAT_00fe1f8d == '\0') || (iVar7 == 0)) {
              FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                           *(undefined2 *)(iVar6 + 0x16a),0x2f6,1,0,0xffffffff,0);
              uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x14,0x32);
              uVar31 = 0;
              uVar18 = 0x303;
LAB_006a8e68:
              uVar14 = *(undefined2 *)(iVar6 + 0x168);
            }
            else {
              uVar31 = 0xffffffff;
              uVar17 = 1;
              if (iVar7 == 1) {
                uVar18 = 0x4e7;
                goto LAB_006a8e68;
              }
              if (iVar7 == 2) {
                uVar18 = 0x314;
                goto LAB_006a8e68;
              }
              if (iVar7 == 3) {
                uVar18 = 0x49a;
                goto LAB_006a8e68;
              }
              uVar14 = *(undefined2 *)(iVar6 + 0x168);
              if (iVar7 == 4) {
                uVar18 = 0x4eb;
              }
              else {
                uVar18 = 0x483;
              }
            }
            FUN_00631590(local_9c,local_98,uVar14,*(undefined2 *)(iVar6 + 0x16a),uVar18,uVar17,0,
                         uVar31,0);
            iVar7 = local_98;
            if (DAT_00fe1f8d == '\0') {
              DAT_00fe1f8d = '\x01';
              FUN_00646f20(0x33,0x32,0xff,0x82,0xffffffff);
              iVar7 = local_98;
            }
            goto LAB_006a99ce;
          }
          if (iVar19 - 0xefU < 2) goto switchD_006a8ba2_caseD_ad;
        }
        else if (iVar19 == 0x12e) goto switchD_006a79a2_caseD_1;
        goto switchD_006a79a2_caseD_5;
      }
      if (iVar19 == 200) {
switchD_006a79a2_caseD_3:
LAB_006a8ffc:
        iVar8 = FUN_00608330(&DAT_00fd67f4,0x32);
        if (iVar8 == 0) {
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0xd8,1,0,0xffffffff,0);
          iVar7 = local_98;
        }
        iVar8 = FUN_00608330(&DAT_00fd67f4,0xfa);
        if (iVar8 == 0) {
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x518,1,0,0xffffffff,0);
          iVar7 = local_98;
          goto LAB_006a99ce;
        }
      }
      else {
        switch(iVar19) {
        case 0xa7:
          goto switchD_006a79a2_caseD_15;
        case 0xad:
        case 0xb5:
switchD_006a8ba2_caseD_ad:
          iVar8 = FUN_00608330(&DAT_00fd67f4,3);
          if (iVar8 == 0) {
            uVar17 = 0;
            iVar8 = 0x532;
            goto LAB_006a7c10;
          }
          break;
        case 0xaf:
          iVar8 = FUN_00608330(&DAT_00fd67f4,200);
          if (iVar8 == 0) {
            iVar8 = 0x4f1;
            goto LAB_006a7c0a;
          }
          break;
        case 0xb8:
          goto switchD_006a79a2_caseD_1;
        case 0xba:
          iVar8 = FUN_005c4e6c(&DAT_00fd67f4,1,10);
          uVar17 = 0x28;
          goto LAB_006a8428;
        case 0xbb:
          uVar17 = FUN_005c4e6c(&DAT_00fd67f4,1,3);
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x17,uVar17,0,0,0);
          iVar8 = FUN_00608330(&DAT_00fd67f4,5000);
          iVar7 = local_98;
          if (iVar8 == 0) {
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x51d,1,0,0xffffffff,0);
            iVar7 = local_98;
          }
          goto LAB_006a8ffc;
        case 0xbc:
        case 0xbd:
          goto switchD_006a79a2_caseD_3;
        case 0xbe:
        case 0xbf:
        case 0xc0:
        case 0xc1:
        case 0xc2:
          goto switchD_006a79a2_caseD_2;
        }
      }
      goto switchD_006a79a2_caseD_5;
    }
    if (0x3f0 < iVar19) {
      switch(iVar19) {
      case 0x3f1:
        iVar8 = FUN_00608330(&DAT_01070468,5);
        if (iVar8 == 0) {
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x210,1,0,0,0);
          iVar7 = local_98;
        }
LAB_006a93fa:
        uVar17 = FUN_005c4e6c(&DAT_01070468,0x28,0x5f);
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x48,uVar17,0,0,0);
        iVar7 = local_98;
        break;
      case 0x3f2:
        uVar17 = FUN_005c4e6c(&DAT_01070468,10,0x23);
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x48,uVar17,0,0,0);
        iVar7 = local_98;
        break;
      case 0x3f3:
        iVar8 = FUN_00608330(&DAT_00fd67f4,10);
        if (iVar8 == 0) {
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x139b,1,0,0,0);
          iVar7 = local_98;
        }
        iVar8 = FUN_00608330(&DAT_00fd67f4,10);
        if (iVar8 != 0) goto switchD_006a79a2_caseD_2a;
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0xd1,2,0,0,0);
        iVar7 = local_98;
        goto LAB_006a99a2;
      case 0x3f4:
        iVar8 = FUN_00608330(&DAT_01070468,8);
        if (iVar8 == 0) {
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x147,1,0,0,0);
          iVar7 = local_98;
        }
        iVar8 = FUN_00608330(&DAT_01070468,2);
        if (iVar8 == 0) {
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x49,1,0,0,0);
          iVar7 = local_98;
        }
        break;
      case 0x3f5:
        uVar17 = FUN_005c4e6c(&DAT_01070468,8,0xf);
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x23f,uVar17,0,0,0);
        iVar7 = FUN_00608330(&DAT_01070468,3);
        if (iVar7 == 0) {
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),0x149,1,0,0,0);
          iVar7 = local_98;
        }
        else {
          uVar17 = FUN_005c4e6c(&DAT_01070468,1,2);
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),0x49,uVar17,0,0,0);
          iVar7 = local_98;
        }
        break;
      default:
        goto switchD_006a79a2_caseD_5;
      case 0x3fb:
        iVar8 = FUN_00608330(&DAT_01070468,0xfa);
        if (iVar8 == 0) {
          uVar17 = 0x206;
          goto LAB_006a82c0;
        }
        iVar8 = FUN_00608330(&DAT_01070468,10);
        if (iVar8 == 0) goto LAB_006a82b6;
        uVar17 = FUN_005c4e6c(&DAT_01070468,5,10);
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x1f6,uVar17,0,0,0);
        iVar7 = local_98;
        break;
      case 0x3fc:
        uVar17 = FUN_005c4e6c(&DAT_00fd67f4,10,0x1e);
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x16e,uVar17,0,0,0);
        uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x14,0x1e);
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x1399,uVar17,0,0,0);
        uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x14,0x32);
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x1395,uVar17,0,0,0);
        iVar8 = FUN_00608330(&DAT_00fd67f4,2);
        iVar7 = local_98;
        if (iVar8 == 0) {
          iVar8 = FUN_00608330(&DAT_00fd67f4,9);
          iVar8 = iVar8 + 0x1389;
          uVar17 = 0;
          uVar18 = 1;
          iVar7 = local_98;
          goto LAB_006a7c16;
        }
        break;
      case 0x400:
        goto switchD_006a79a2_caseD_2f;
      }
      goto LAB_006a99ce;
    }
    if (iVar19 == 0x3f0) goto LAB_006a9554;
    if (iVar19 < 0x3ec) {
      if (iVar19 == 0x3eb) {
        iVar8 = FUN_00608330(&DAT_00fd67f4,10);
        if (iVar8 == 0) {
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x139c,1,0,0,0);
          iVar7 = local_98;
        }
        iVar8 = FUN_00608330(&DAT_00fd67f4,10);
        if (iVar8 == 0) {
          iVar8 = FUN_005c4e6c(&DAT_00fd67f4,0x19a,0x19c);
          uVar17 = 0;
          uVar18 = 1;
          goto LAB_006a7c16;
        }
      }
      else if (iVar19 < 0x3ea) {
        if (iVar19 == 0x3e9) goto switchD_006a79a2_caseD_45;
        switch(iVar19) {
        case 0x13f:
        case 0x140:
        case 0x141:
          goto switchD_006a79a2_caseD_3;
        case 0x142:
        case 0x143:
        case 0x144:
          goto switchD_006a79a2_caseD_15;
        }
      }
      else if ((iVar19 == 0x3ea) && (iVar8 = FUN_00608330(&DAT_00fd67f4,0x19), iVar8 == 0)) {
        uVar17 = 0;
        iVar8 = 0x10c;
        goto LAB_006a7c10;
      }
      goto switchD_006a79a2_caseD_5;
    }
    switch(iVar19) {
    case 0x3ec:
      uVar17 = FUN_005c4e6c(&DAT_01070468,5,8);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x17
                   ,uVar17,0,0,0);
      uVar17 = FUN_005c4e6c(&DAT_01070468,0x14,0x4d);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x48,uVar17,0,0,0);
      iVar8 = FUN_00608330(&DAT_00fd67f4,0x14);
      iVar7 = local_98;
      if (iVar8 == 0) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x139a,1,0,0,0);
        iVar7 = local_98;
      }
      break;
    case 0x3ed:
      iVar8 = FUN_00608330(&DAT_01070468,6);
      if (iVar8 == 0) {
        uVar17 = FUN_005c4e6c(&DAT_01070468,2,5);
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x187,uVar17,0,0,0);
        iVar7 = local_98;
      }
      else {
        iVar8 = FUN_00608330(&DAT_01070468,3);
        if (iVar8 == 0) {
          uVar17 = FUN_005c4e6c(&DAT_01070468,2,5);
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x17e,uVar17,0,0,0);
          iVar7 = local_98;
        }
        else {
          uVar17 = FUN_005c4e6c(&DAT_01070468,2,5);
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x17d,uVar17,0,0,0);
          iVar7 = local_98;
        }
      }
      break;
    case 0x3ee:
      iVar8 = FUN_00608330(&DAT_01070468,5);
      if (iVar8 == 0) {
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x20f,1,0,0,0);
        iVar7 = local_98;
      }
      goto LAB_006a93fa;
    case 0x3ef:
      uVar17 = FUN_005c4e6c(&DAT_01070468,9,0x23);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x17
                   ,uVar17,0,0,0);
      uVar17 = FUN_005c4e6c(&DAT_01070468,10,0x25);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x48,uVar17,0,0,0);
      iVar7 = local_98;
      break;
    default:
      goto switchD_006a79a2_caseD_5;
    }
    goto LAB_006a99ce;
  }
  if (iVar19 == 0x93) {
switchD_006a79a2_caseD_1:
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,1,3);
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x17,
                 uVar17,0,0,0);
    iVar7 = FUN_00608330(&DAT_00fd67f4,3000);
    if (iVar7 == 0) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x51d,1,0,0xffffffff,0);
    }
    iVar7 = local_98;
    if ((*(int *)(iVar6 + 0x104) == 0xcc) &&
       (iVar8 = FUN_00608330(&DAT_00fd67f4,3), iVar7 = local_98, iVar8 == 0)) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0xd1,1,0,0,0);
      iVar7 = local_98;
    }
    goto LAB_006a99ce;
  }
  switch(iVar19) {
  case 1:
  case 0x10:
  case 0x8a:
  case 0x8d:
    goto switchD_006a79a2_caseD_1;
  case 2:
    goto switchD_006a79a2_caseD_2;
  case 3:
  case 0x84:
    goto switchD_006a79a2_caseD_3;
  case 4:
    iVar8 = FUN_00608330(&DAT_00fd67f4,0x96);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x513,1,0,0,0);
      iVar7 = local_98;
    }
    if (DAT_0102a787 == '\0') {
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x14,0x32);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x2f
                   ,uVar17,0,0,0);
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,10,0x1e);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x38,uVar17,0,0,0);
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,10,0x1e);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x38,uVar17,0,0,0);
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,10,0x1e);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x38,uVar17,0,0,0);
      iVar8 = FUN_005c4e6c(&DAT_00fd67f4,1,4);
      uVar17 = 0x3b;
      iVar7 = local_98;
    }
    else {
      iVar8 = FUN_00608330(&DAT_00fd67f4,0x14);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x370,iVar8 + 10,0,0,0);
      iVar7 = FUN_00608330(&DAT_00fd67f4,0x14);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x370,iVar7 + 10,0,0,0);
      iVar8 = FUN_00608330(&DAT_00fd67f4,0x14);
      iVar8 = iVar8 + 10;
      uVar17 = 0x370;
      iVar7 = local_98;
    }
    goto LAB_006a8428;
  default:
    goto switchD_006a79a2_caseD_5;
  case 6:
  case 0x5e:
    iVar8 = FUN_00608330(&DAT_00fd67f4,3);
    if (iVar8 == 0) {
      uVar17 = 0;
      iVar8 = 0x44;
      goto LAB_006a7c10;
    }
    goto switchD_006a79a2_caseD_5;
  case 7:
  case 8:
  case 9:
    iVar8 = FUN_00608330(&DAT_00fd67f4,3);
    if (iVar8 == 0) {
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,1,3);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x44
                   ,uVar17,0,0,0);
      iVar8 = FUN_00608330(&DAT_00fd67f4,3);
      iVar7 = local_98;
      if (iVar8 == 0) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x13c3,1,0,0,0);
        iVar7 = local_98;
      }
    }
    uVar18 = FUN_005c4e6c(&DAT_00fd67f4,3,9);
    uVar17 = 0;
    iVar8 = 0x45;
    goto LAB_006a7c16;
  case 10:
  case 0xb:
  case 0xc:
  case 0x5f:
  case 0x60:
  case 0x61:
    iVar8 = FUN_00608330(&DAT_00fd67f4,100);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0xd7
                   ,1,0,0,0);
      iVar7 = local_98;
    }
    iVar8 = FUN_00608330(&DAT_01070468,0x32);
    if (iVar8 == 0) {
      uVar17 = FUN_005c4e6c(&DAT_01070468,5,10);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x43
                   ,uVar17,0,0,0);
      iVar7 = local_98;
    }
    iVar8 = FUN_00608330(&DAT_01070468,10);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x13c3,1,0,0,0);
      iVar7 = local_98;
    }
    break;
  case 0xd:
  case 0xe:
  case 0xf:
    iVar8 = FUN_00608330(&DAT_00fd67f4,2);
    if (iVar8 == 0) {
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,1,3);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x56
                   ,uVar17,0,0,0);
      iVar7 = local_98;
    }
    iVar8 = FUN_00608330(&DAT_01070468,3);
    if (iVar8 == 0) {
      uVar17 = FUN_005c4e6c(&DAT_01070468,1,3);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x44
                   ,uVar17,0,0,0);
      iVar7 = local_98;
    }
    iVar8 = FUN_00608330(&DAT_01070468,2);
    if (iVar8 == 0) {
      uVar17 = FUN_005c4e6c(&DAT_01070468,2,6);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x38
                   ,uVar17,0,0,0);
      iVar7 = local_98;
    }
    if (*(char *)(iVar6 + 0x110) != '\0') {
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,10,0x1e);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x38
                   ,uVar17,0,0,0);
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,10,0x1f);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x38,uVar17,0,0,0);
      iVar7 = FUN_00608330(&DAT_00fd67f4,0x1e);
      if (iVar7 == 0) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x3e2,1,0,0,0);
      }
      iVar8 = FUN_00608330(&DAT_01070468,10);
      iVar7 = local_98;
      if (iVar8 == 0) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x13c3,1,0,0,0);
        iVar7 = local_98;
      }
    }
    iVar8 = FUN_00608330(&DAT_01070468,3);
    if (iVar8 == 0) {
      FUN_006c314c(&local_9c);
      iVar8 = FUN_006be868();
      iVar7 = local_98;
      if (iVar8 != 0) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x3a,1,0,0,0);
        iVar7 = local_98;
      }
    }
    break;
  case 0x15:
  case 0x2c:
switchD_006a79a2_caseD_15:
    iVar8 = FUN_00608330(&DAT_00fd67f4,0x19);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x76
                   ,1,0,0,0);
      iVar7 = local_98;
    }
    if ((*(int *)(iVar6 + 0x104) == 0x15) || (*(int *)(iVar6 + 0x104) == 0x6e)) {
      uVar17 = FUN_005c4e6c(&DAT_01070468,1,4);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x9a
                   ,uVar17,0,0,0);
      iVar7 = local_98;
    }
    if (*(int *)(iVar6 + 0x104) == 0x2c) {
      iVar8 = FUN_00608330(&DAT_00fd67f4,0x14);
      if (iVar8 == 0) {
        uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x19a,0x19c);
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     uVar17,1,0,0,0);
        iVar7 = local_98;
      }
      else {
        uVar17 = FUN_005c4e6c(&DAT_01070468,1,4);
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0xa6,uVar17,0,0,0);
        iVar7 = local_98;
      }
    }
    else {
      if (*(int *)(iVar6 + 0x104) != 0x3eb) goto switchD_006a79a2_caseD_5;
      uVar17 = FUN_005c4e6c(&DAT_01070468,0xf,0x37);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x48
                   ,uVar17,0,0,0);
      iVar8 = FUN_00608330(&DAT_01070468,10);
      iVar7 = local_98;
      if (iVar8 == 0) {
        uVar17 = FUN_005c4e6c(&DAT_01070468,0x19a,0x19c);
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,uVar17,1,0,0,0);
        iVar7 = local_98;
      }
    }
    break;
  case 0x17:
    iVar8 = FUN_00608330(&DAT_00fd67f4,0x32);
    if (iVar8 == 0) {
      uVar17 = 0;
      iVar8 = 0x74;
      goto LAB_006a7c10;
    }
    goto switchD_006a79a2_caseD_5;
  case 0x18:
    iVar8 = FUN_00608330(&DAT_00fd67f4,300);
    if (iVar8 == 0) {
      uVar17 = 0;
      iVar8 = 0xf4;
      goto LAB_006a7c10;
    }
    goto switchD_006a79a2_caseD_5;
  case 0x1a:
  case 0x1b:
  case 0x1c:
  case 0x1d:
  case 0x6f:
    iVar8 = FUN_00608330(&DAT_00fd67f4,200);
    if (iVar8 < 100) {
      if (iVar8 == 0) {
        uVar17 = 0;
        iVar8 = 0xa0;
        goto LAB_006a7c10;
      }
      iVar8 = FUN_005c4e6c(&DAT_00fd67f4,1,6);
      uVar17 = 0xa1;
      goto LAB_006a8428;
    }
    goto switchD_006a79a2_caseD_5;
  case 0x1f:
  case 0x20:
    iVar8 = FUN_00608330(&DAT_00fd67f4,0xfa);
    if (iVar8 == 0) {
      uVar17 = 0;
      iVar8 = 0x3a4;
      goto LAB_006a7c10;
    }
    iVar8 = FUN_00608330(&DAT_01070468,0x14);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x147,1,0,0,0);
      iVar7 = local_98;
    }
    else {
      uVar17 = FUN_005c4e6c(&DAT_01070468,1,4);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x9a
                   ,uVar17,0,0,0);
      iVar7 = local_98;
    }
    break;
  case 0x22:
    iVar8 = FUN_00608330(&DAT_01070468,0x11);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x147,1,0,0,0);
      iVar7 = local_98;
    }
    break;
  case 0x2a:
switchD_006a79a2_caseD_2a:
LAB_006a99a2:
    iVar8 = 0xd1;
    iVar19 = FUN_00608330(&DAT_00fd67f4,2);
    if (iVar19 == 0) goto switchD_006a79a2_caseD_42;
    goto switchD_006a79a2_caseD_5;
  case 0x2b:
    iVar8 = FUN_00608330(&DAT_00fd67f4,3);
    if (iVar8 == 0) {
      uVar17 = 0;
      iVar8 = 0xd2;
      goto LAB_006a7c10;
    }
    goto switchD_006a79a2_caseD_5;
  case 0x2d:
    uVar17 = 0;
    iVar8 = 0xee;
    goto LAB_006a7c10;
  case 0x2f:
switchD_006a79a2_caseD_2f:
    iVar7 = FUN_00735520();
    if (iVar7 != 0) {
      fVar27 = *(float *)(iVar6 + 0x1ec);
      uVar15 = uVar10 & 0xfffffff | (uint)(fVar27 < 15.0) << 0x1f | (uint)(fVar27 == 15.0) << 0x1e;
      uVar10 = uVar15 | (uint)NAN(fVar27) << 0x1c;
      bVar4 = (byte)(uVar15 >> 0x18);
      if (!(bool)(bVar4 >> 6 & 1) && bVar4 >> 7 == ((byte)(uVar10 >> 0x1c) & 1)) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x13c0,1,0,0,0);
      }
    }
    iVar8 = FUN_00608330(&DAT_01070468,0x4b);
    iVar7 = local_98;
    if (iVar8 == 0) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0xf3,1,0,0,0);
      iVar7 = local_98;
    }
    break;
  case 0x30:
    iVar8 = FUN_00608330(&DAT_00fd67f4,2);
    if (iVar8 == 0) {
      uVar17 = 0;
      iVar8 = 0x140;
      goto LAB_006a7c10;
    }
    goto switchD_006a79a2_caseD_5;
  case 0x32:
    iVar8 = FUN_005c4e6c(&DAT_00fd67f4,0x100,0x103);
    uVar18 = 1;
    uVar17 = 0;
    goto LAB_006a7c16;
  case 0x34:
    uVar17 = 0;
    iVar8 = 0xfb;
    goto LAB_006a7c10;
  case 0x35:
    iVar8 = FUN_00608330(&DAT_00fd67f4,5);
    if (iVar8 == 0) {
      iVar8 = 0x139e;
    }
    else {
      iVar8 = 0xef;
    }
    uVar17 = 0;
    goto LAB_006a7c10;
  case 0x36:
    uVar17 = 0;
    iVar8 = 0x104;
    goto LAB_006a7c10;
  case 0x37:
switchD_006a79a2_caseD_37:
    uVar17 = 0;
    iVar8 = 0x105;
    goto LAB_006a7c10;
  case 0x3a:
    iVar8 = FUN_00608330(&DAT_00fd67f4,500);
    if (0xc < iVar8) goto switchD_006a79a2_caseD_5;
    if (iVar8 == 0) {
      uVar17 = 0x107;
    }
    else {
      uVar17 = 0x76;
    }
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),uVar17
                 ,1,0,0,0);
    iVar7 = local_98;
    break;
  case 0x3e:
    iVar8 = FUN_00608330(&DAT_01070468,0x32);
    if (iVar8 == 0) {
LAB_006a82b6:
      uVar17 = 0x110;
LAB_006a82c0:
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   uVar17,1,0,0xffffffff,0);
      iVar7 = local_98;
    }
    break;
  case 0x3f:
  case 0x40:
  case 0x67:
    iVar8 = FUN_00608330(&DAT_00fd67f4,100);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x517,1,0,0xffffffff,0);
      iVar7 = local_98;
    }
    uVar18 = FUN_005c4e6c(&DAT_00fd67f4,1,5);
    uVar17 = 0;
    iVar8 = 0x11a;
    goto LAB_006a7c16;
  case 0x41:
    iVar8 = FUN_00608330(&DAT_00fd67f4,0x32);
    uVar14 = *(undefined2 *)(iVar6 + 0x168);
    if (iVar8 == 0) {
      uVar18 = 1;
      uVar17 = 0;
      iVar8 = 0x10c;
    }
    else {
      uVar18 = 1;
      uVar17 = 0;
      iVar8 = 0x13f;
    }
    goto LAB_006a7c1e;
  case 0x42:
switchD_006a79a2_caseD_42:
    uVar18 = 1;
    uVar17 = 0;
    goto LAB_006a7c16;
  case 0x45:
switchD_006a79a2_caseD_45:
    iVar8 = FUN_00608330(&DAT_01070468,2);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x143,1,0,0,0);
      iVar7 = local_98;
    }
    break;
  case 0x47:
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x147,
                 1,0,0,0);
    iVar7 = local_98;
    break;
  case 0x49:
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,1,3);
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x16a,
                 uVar17,0,0,0);
    iVar7 = local_98;
    break;
  case 0x4b:
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,1,4);
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x1f5,
                 uVar17,0,0,0);
    iVar7 = local_98;
    break;
  case 0x4f:
    iVar8 = FUN_00608330(&DAT_01070468,10);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x20f,1,0,0,0);
      iVar7 = local_98;
    }
    break;
  case 0x50:
    iVar8 = FUN_00608330(&DAT_01070468,10);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x210,1,0,0,0);
      iVar7 = local_98;
    }
    break;
  case 0x51:
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,2,5);
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x17,
                 uVar17,0,0,0);
    iVar7 = local_98;
    break;
  case 0x53:
    iVar8 = FUN_00608330(&DAT_01070468,0xc);
    if (iVar8 == 0) {
      uVar17 = FUN_005c4e6c(&DAT_01070468,2,5);
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x187,uVar17,0,0,0);
      iVar7 = local_98;
    }
    else {
      iVar8 = FUN_00608330(&DAT_01070468,6);
      if (iVar8 == 0) {
        uVar17 = FUN_005c4e6c(&DAT_01070468,2,5);
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x17e,uVar17,0,0,0);
        iVar7 = local_98;
      }
      else {
        iVar8 = FUN_00608330(&DAT_01070468,3);
        if (iVar8 == 0) {
          uVar17 = FUN_005c4e6c(&DAT_01070468,2,5);
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x17d,uVar17,0,0,0);
          iVar7 = local_98;
        }
      }
    }
    break;
  case 0x55:
    fVar27 = *(float *)(iVar6 + 0x1ec);
    uVar15 = uVar10 & 0xfffffff;
    uVar9 = uVar15 | (uint)(fVar27 < 0.0) << 0x1f | (uint)(fVar27 == 0.0) << 0x1e;
    uVar10 = uVar9 | (uint)NAN(fVar27) << 0x1c;
    bVar4 = (byte)(uVar9 >> 0x18);
    if (!(bool)(bVar4 >> 6 & 1) && bVar4 >> 7 == ((byte)(uVar10 >> 0x1c) & 1)) {
      uVar10 = uVar15 | (uint)(*(float *)(iVar6 + 0x184) == 4.0) << 0x1e;
      if ((byte)(uVar10 >> 0x1e) == 0) {
        uVar17 = FUN_00608330(&DAT_00fd67f4,7);
        switch(uVar17) {
        case 0:
          iVar8 = 0x1b5;
          break;
        case 1:
          iVar8 = 0x205;
          break;
        case 2:
          iVar8 = 0x217;
          break;
        case 3:
          iVar8 = 0x218;
          break;
        case 4:
          iVar8 = 0x214;
          break;
        case 5:
          uVar17 = FUN_005c4e6c(&DAT_01070468,1,2);
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x231,uVar17,0,0xffffffff,0);
          iVar7 = local_98;
          goto LAB_006a99ce;
        default:
          iVar8 = 0x22a;
        }
      }
      else {
        iVar8 = FUN_00608330(&DAT_00fd67f4,0x14);
        if (iVar8 == 0) {
          iVar8 = 0x520;
        }
        else {
          iVar8 = FUN_00608330(&DAT_00fd67f4,3);
          if (iVar8 == 0) {
            iVar8 = 0x2a4;
          }
          else if (iVar8 == 1) {
            iVar8 = 0x2d5;
          }
          else {
            if (iVar8 != 2) goto switchD_006a79a2_caseD_5;
            iVar8 = 0x4f0;
          }
        }
      }
LAB_006a7c0a:
      uVar17 = 0xffffffff;
      goto LAB_006a7c10;
    }
    goto switchD_006a79a2_caseD_5;
  case 0x56:
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x20e,
                 1,0,0,0);
    iVar8 = FUN_00608330(&DAT_00fd67f4,100);
    iVar7 = local_98;
    if (iVar8 == 0) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x358,1,0,0,0);
      iVar7 = local_98;
    }
    break;
  case 0x57:
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,5,0xb);
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x23f,
                 uVar17,0,0,0);
    iVar7 = local_98;
    break;
  case 0x62:
  case 0x65:
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,2,6);
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x20a,
                 uVar17,0,0,0);
    iVar7 = local_98;
    break;
  case 0x66:
    iVar8 = FUN_00608330(&DAT_00fd67f4,500);
    if (iVar8 != 0) goto switchD_006a79a2_caseD_5;
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x107,
                 1,0,0,0);
    iVar7 = local_98;
    break;
  case 0x68:
    iVar8 = FUN_00608330(&DAT_00fd67f4,0x3c);
    if (iVar8 == 0) {
      iVar8 = 0x1e5;
      goto LAB_006a7c0a;
    }
    iVar8 = FUN_00608330(&DAT_00fd67f4,0x14);
    if (iVar8 != 0) goto switchD_006a79a2_caseD_5;
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x139d
                 ,1,0,0,0);
    iVar7 = local_98;
    break;
  case 0x6d:
    if ((DAT_00fe1f85 == '\0') && (DAT_00fe1f85 = '\x01', DAT_00fe1ec4 == 2)) {
      FUN_00647dc0(0);
      iVar7 = local_98;
    }
switchD_006a79a2_caseD_5:
    break;
  case 0x71:
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x16f,
                 1,0,0xffffffff,0);
    iVar7 = FUN_00608330(&DAT_00fd67f4,2);
    if (iVar7 == 0) {
      uVar17 = FUN_005c4e6c(&DAT_00fd67f4,0x1e9,0x1ec);
    }
    else {
      iVar7 = FUN_00608330(&DAT_00fd67f4,3);
      if (iVar7 == 0) {
        uVar17 = 0x202;
      }
      else if (iVar7 == 1) {
        uVar17 = 0x1aa;
      }
      else {
        uVar17 = 0x1b2;
      }
    }
    FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                 uVar17,1,0,0xffffffff,0);
    iVar19 = (*(ushort *)(iVar6 + 0x168) >> 5) + 1;
    iVar8 = *(int *)(iVar6 + 0x148) + (*(int *)(iVar6 + 0x150) >> 1) >> 4;
    iVar24 = iVar8 - iVar19;
    iVar8 = iVar19 + iVar8;
    iVar7 = local_98;
    if (iVar24 <= iVar8) {
      iVar1 = *(int *)(iVar6 + 0x14c) + (*(int *)(iVar6 + 0x154) >> 1) >> 4;
      iVar22 = iVar1 - iVar19;
      iVar19 = iVar19 + iVar1;
      iVar1 = iVar22;
      iVar23 = iVar24;
      do {
        for (; local_98 = iVar7, iVar1 <= iVar19; iVar1 = iVar1 + 1) {
          bVar25 = false;
          iVar7 = (DAT_00902934 * iVar23 + iVar1) * 0xe + DAT_00902928;
          if (((((iVar23 == iVar24) || (iVar23 == iVar8)) || (iVar1 == iVar22)) || (iVar1 == iVar19)
              ) && ((*(byte *)(iVar7 + 1) & 1) == 0)) {
            *(byte *)(iVar7 + 1) = *(byte *)(iVar7 + 1) | 1;
            *(undefined2 *)(iVar7 + 6) = 0x8c;
            FUN_007729e8(iVar23,iVar1,0xffffffff);
            bVar25 = true;
          }
          if (*(char *)(iVar7 + 4) != '\0') {
            *(ushort *)(iVar7 + 2) = *(ushort *)(iVar7 + 2) & 0xcfff;
            *(undefined1 *)(iVar7 + 4) = 0;
            bVar25 = true;
          }
          if (bVar25) {
            FUN_00649a94(iVar23,iVar1,0,0);
          }
          iVar7 = local_98;
        }
        iVar23 = iVar23 + 1;
        iVar1 = iVar22;
      } while (iVar23 <= iVar8);
      goto switchD_006a79a2_caseD_5;
    }
    break;
  case 0x74:
  case 0x75:
  case 0x76:
  case 0x77:
  case 0x8b:
    uVar17 = 0;
    iVar8 = 0x3a;
LAB_006a7c10:
    uVar18 = 1;
LAB_006a7c16:
    uVar14 = *(undefined2 *)(iVar6 + 0x168);
LAB_006a7c1e:
    FUN_00631590(local_9c,iVar7,uVar14,*(undefined2 *)(iVar6 + 0x16a),iVar8,uVar18,0,uVar17,0);
    iVar7 = local_98;
    break;
  case 0x78:
    iVar8 = FUN_00608330(&DAT_01070468,0x32);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x1ef,1,0,0,0);
      iVar7 = local_98;
    }
LAB_006a9554:
    iVar8 = FUN_00608330(&DAT_01070468,3);
    if (iVar8 == 0) {
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0x208,1,0,0,0);
      iVar7 = local_98;
    }
    break;
  case 0x7a:
    uVar17 = FUN_005c4e6c(&DAT_01070468,5,0xb);
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x17,
                 uVar17,0,0,0);
    iVar7 = local_98;
    break;
  case 0x7d:
  case 0x7e:
    if (iVar19 == 0x7d) {
      uVar17 = 0x7e;
    }
    else {
      uVar17 = 0x7d;
    }
    iVar8 = FUN_00656370(uVar17);
    if (iVar8 == 0) {
      uVar18 = FUN_005c4e6c(&DAT_00fd67f4,0x14,0x1f);
      uVar17 = 0x225;
      goto LAB_006a8b0c;
    }
    *(undefined4 *)(iVar6 + 0x1ec) = DAT_006a8b5c;
    *(undefined1 *)(iVar6 + 0x110) = 0;
    iVar7 = local_98;
    break;
  case 0x7f:
    uVar18 = FUN_005c4e6c(&DAT_00fd67f4,0x14,0x1f);
    uVar17 = 0x223;
    goto LAB_006a8b0c;
  case 0x86:
    uVar18 = FUN_005c4e6c(&DAT_00fd67f4,0x14,0x1f);
    uVar17 = 0x224;
LAB_006a8b0c:
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),uVar17
                 ,uVar18,0,0,0);
    iVar8 = FUN_005c4e6c(&DAT_00fd67f4,0x14,0x24);
    uVar17 = 0x4c9;
    iVar7 = local_98;
LAB_006a8428:
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),uVar17
                 ,iVar8,0,0,0);
    iVar7 = local_98;
    break;
  case 0x8f:
  case 0x90:
  case 0x91:
    uVar17 = FUN_005c4e6c(&DAT_00fd67f4,5,0xb);
    FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),0x251,
                 uVar17,0,0,0);
    iVar7 = local_98;
  }
LAB_006a99ce:
  if (((*(char *)(iVar6 + 0x110) != '\0') || (*(int *)(iVar6 + 0x104) == 0x7d)) ||
     (*(int *)(iVar6 + 0x104) == 0x7e)) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    if ((int)(fVar27 * fVar26 * 10.0) == 0) {
      iVar8 = *(int *)(iVar6 + 0x104);
      if (iVar8 == 4) {
        uVar17 = 0x550;
      }
      else if (((iVar8 == 0xd) || (iVar8 == 0xe)) || (iVar8 == 0xf)) {
        uVar17 = 0x551;
      }
      else if (iVar8 == 0x10a) {
        uVar17 = 0x552;
      }
      else if (iVar8 == 0x23) {
        uVar17 = 0x553;
      }
      else if (iVar8 == 0xde) {
        uVar17 = 0x554;
      }
      else if (iVar8 == 0x71) {
        uVar17 = 0x555;
      }
      else if (iVar8 == 0x86) {
        uVar17 = 0x556;
      }
      else if (iVar8 == 0x7f) {
        uVar17 = 0x557;
      }
      else if (iVar8 == 0x7d) {
        uVar17 = 0x558;
      }
      else if (iVar8 == 0x7e) {
        uVar17 = 0x559;
      }
      else if (iVar8 == 0x106) {
        uVar17 = 0x55a;
      }
      else if (iVar8 == 0xf5) {
        uVar17 = 0x55b;
      }
      else {
        if (iVar8 != 0x3fc) goto LAB_006a9af2;
        uVar17 = 0x13ac;
      }
      FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   uVar17,1,0,0,0);
      iVar7 = local_98;
    }
  }
LAB_006a9af2:
  if (((*(char *)(iVar6 + 0x110) != '\0') || (*(int *)(iVar6 + 0x104) == 0x147)) ||
     (*(int *)(iVar6 + 0x104) == 0x145)) {
    sVar3 = *(short *)(iVar6 + 0x1f0);
    if (0 < *(int *)(iVar6 + 0x5c)) {
      sVar3 = *(short *)(*(int *)(iVar6 + 0x5c) * 0x274 + DAT_00fe1fa8 + 0x1f0);
    }
    FUN_0072701c((int)sVar3);
    FUN_00736e98();
    iVar7 = local_98;
  }
  if (*(char *)(iVar6 + 0x110) != '\0') {
    FUN_00595220(auStack_8c,iVar6 + 0x234);
    iVar7 = *(int *)(iVar6 + 0x104);
    if (iVar7 < 0x80) {
      if (iVar7 == 0x7f) {
        FUN_00736ecc(0xd);
        DAT_00fe1f8a = '\x01';
        DAT_00fe1f8b = '\x01';
      }
      else if (iVar7 < 0x24) {
        if (iVar7 == 0x23) {
          DAT_00fe1f7b = 1;
        }
        else if (iVar7 == 4) {
          DAT_00fe1f73 = 1;
        }
        else if (iVar7 - 0xdU < 3) {
          DAT_00fe1f7a = 1;
        }
      }
      else if (iVar7 - 0x7dU < 2) {
        DAT_00fe1f89 = '\x01';
        DAT_00fe1f8b = '\x01';
        FUN_007aa80c(auStack_8c,auStack_fc,&DAT_00fd6d08);
        FUN_007aa754(auStack_fc);
        FUN_00736ecc(0xc);
      }
    }
    else if (iVar7 < 0x3ff) {
      if (iVar7 == 0x3fe) {
        iVar8 = FUN_00656030();
        iVar7 = DAT_006aa14c;
        if (iVar8 != 1) goto LAB_006a9d04;
        do {
          if (*(int *)(iVar7 + DAT_00fe1fa8 + 0x104) == 0x3ff) {
            *(undefined1 *)(iVar7 + DAT_00fe1fa8 + 0x100) = 0;
          }
          iVar7 = iVar7 + -0x274;
        } while (-1 < iVar7);
        DAT_00fe1f86 = 1;
        uVar17 = FUN_0041c4d8(auStack_15c,&DAT_0080ed7b);
        uVar18 = FUN_0041c4d8(auStack_18c,&DAT_0080ed7b);
        uVar31 = FUN_0041c4d8(auStack_12c,"EasterBunniesKilled");
        FUN_004c9784(uVar31,uVar18,uVar17);
        iVar7 = FUN_00608330(&DAT_01070468,3);
        if (iVar7 == 0) {
          uVar17 = 0x13c2;
        }
        else {
          uVar17 = 0x13c1;
        }
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,uVar17,1,0,0,0);
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x13c0,1,0,0,0);
      }
      else if (iVar7 == 0x86) {
        FUN_00736ecc(0xe);
        DAT_00fe1f88 = '\x01';
        DAT_00fe1f8b = '\x01';
      }
    }
    else if (iVar7 - 0x401U < 2) {
      iVar7 = FUN_00656030(0x402);
      iVar8 = FUN_00656030(0x401);
      if (iVar8 + iVar7 == 1) {
        DAT_00fe1f8f = 1;
        uVar17 = FUN_0041c4d8(auStack_174,&DAT_0080ed7b);
        uVar18 = FUN_0041c4d8(auStack_144,&DAT_0080ed7b);
        uVar31 = FUN_0041c4d8(auStack_114,"TurkorsKilled");
        FUN_004c9784(uVar31,uVar18,uVar17);
        iVar7 = FUN_00608330(&DAT_01070468,5);
        if (1 < iVar7) {
          iVar7 = iVar7 + -1;
          do {
            FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                         *(undefined2 *)(iVar6 + 0x16a),0x140,1,0,0xffffffff,0);
            iVar7 = iVar7 + -1;
          } while (iVar7 != 0);
        }
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x13c9,1,0,0xffffffff,0);
      }
      else {
LAB_006a9d04:
        *(undefined1 *)(iVar6 + 0x110) = 0;
      }
    }
    if (*(char *)(iVar6 + 0x110) != '\0') {
      iVar7 = *(int *)(iVar6 + 0x104);
      uVar17 = 0x1c;
      if (iVar7 == 0x71) {
        uVar17 = 0xbc;
      }
      else if (iVar7 == 0xde) {
        uVar17 = 0x46e;
      }
      else if ((((0x71 < iVar7) && (iVar7 < 0xde)) || (iVar7 == 0xf5)) || (iVar7 == 0x106)) {
        uVar17 = 499;
      }
      uVar18 = FUN_005c4e6c(&DAT_00fd67f4,5,0x10);
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   uVar17,uVar18,0,0,0);
      for (iVar7 = FUN_005c4e6c(&DAT_00fd67f4,5,10); 0 < iVar7; iVar7 = iVar7 + -1) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0x3a,1,0,0,0);
      }
      if (*(int *)(iVar6 + 0x104) == 0x71) {
        FUN_00767524();
      }
      uVar17 = FUN_00595220(auStack_cc,auStack_8c);
      FUN_0064a758(uVar17,0x11,0xaf,0x4b,0xff,0xffffffff);
      FUN_00647dc0(0);
    }
    FUN_007aa754(auStack_8c);
    iVar7 = local_98;
  }
  if ((cVar5 == '\0') && (DAT_00fe1f8b != '\0')) {
    FUN_00646f20(0x32,0x32,0xff,0x82,0xffffffff);
    FUN_007832fc();
    FUN_007832fc();
    FUN_007832fc();
    iVar7 = local_98;
  }
  uVar15 = DAT_00fd6800;
  if ((1 < *(int *)(iVar6 + 0x1b8)) && (0 < *(int *)(iVar6 + 0x19c))) {
    uVar17 = FUN_006c314c(&local_9c);
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = uVar15 ^ DAT_00fd6800 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8;
    DAT_00fd67fc = DAT_00fd6800;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    DAT_00fd6800 = uVar15;
    if ((int)(fVar27 * fVar26 * 6.0) == 0) {
      iVar7 = FUN_00608330(&DAT_00fd67f4,2);
      if ((iVar7 == 0) && (iVar7 = FUN_006be850(uVar17), iVar7 != 0)) {
        FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a)
                     ,0xb8,1,0,0,0);
      }
      else {
        iVar7 = FUN_00608330(&DAT_00fd67f4,2);
        if ((iVar7 == 0) && (iVar7 = FUN_006be868(uVar17), iVar7 != 0)) {
          FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),
                       *(undefined2 *)(iVar6 + 0x16a),0x3a,1,0,0,0);
        }
      }
    }
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar15 = DAT_00fd6800 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar15;
    fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    iVar7 = local_98;
    DAT_00fd67fc = DAT_00fd6800;
    if (((int)(fVar27 * fVar26 * 2.0) == 0) &&
       (DAT_00fd6800 = uVar15, iVar8 = FUN_006be850(uVar17), iVar7 = local_98, uVar15 = DAT_00fd6800
       , iVar8 != 0)) {
      FUN_00631590(local_9c,local_98,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                   0xb8,1,0,0,0);
      iVar7 = local_98;
      uVar15 = DAT_00fd6800;
    }
  }
  DAT_00fd6800 = uVar15;
  fVar27 = *(float *)(iVar6 + 0x1ec);
  if (*(char *)(iVar6 + 0xf2) != '\0') {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = DAT_00fd6800;
    DAT_00fd6800 = DAT_00fd6800 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar15;
    fVar28 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    fVar28 = (float)VectorSignedToFloat(10 - (int)(fVar28 * fVar26 * DAT_006aa144),
                                        (byte)(uVar10 >> 0x16) & 3);
    fVar27 = (fVar28 * DAT_006aa148 + 1.0) * fVar27;
  }
  uVar11 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
  uVar11 = uVar11 ^ DAT_00fd6800 ^ (uVar11 ^ DAT_00fd6800 >> 0xb) >> 8;
  fVar28 = (float)VectorSignedToFloat(uVar11 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
  uVar16 = DAT_00fd67f8 ^ DAT_00fd67f8 << 0xb;
  fVar28 = (float)VectorSignedToFloat(-0x14 - (int)(fVar28 * fVar26 * DAT_006aa140),
                                      (byte)(uVar10 >> 0x16) & 3);
  uVar16 = uVar11 ^ (uVar16 ^ uVar11 >> 0xb) >> 8 ^ uVar16;
  fVar27 = (fVar28 * DAT_006aa148 + 1.0) * fVar27;
  fVar28 = (float)VectorSignedToFloat(uVar16 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
  uVar15 = uVar11;
  uVar9 = DAT_00fd6800;
  DAT_00fd67f4 = uVar16;
  if ((int)(fVar28 * fVar26 * 5.0) == 0) {
    uVar15 = DAT_00fd67fc ^ DAT_00fd67fc << 0xb;
    DAT_00fd67f4 = uVar15 ^ uVar16 ^ (uVar15 ^ uVar16 >> 0xb) >> 8;
    fVar28 = (float)VectorSignedToFloat(DAT_00fd67f4 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    fVar28 = (float)VectorSignedToFloat(5 - (int)(fVar28 * fVar26 * -6.0),(byte)(uVar10 >> 0x16) & 3
                                       );
    fVar27 = (fVar28 * DAT_006aa148 + 1.0) * fVar27;
    uVar15 = uVar16;
    uVar9 = uVar11;
    DAT_00fd67fc = DAT_00fd6800;
  }
  uVar16 = DAT_00fd67fc ^ DAT_00fd67fc << 0xb;
  uVar16 = DAT_00fd67f4 ^ (uVar16 ^ DAT_00fd67f4 >> 0xb) >> 8 ^ uVar16;
  fVar28 = (float)VectorSignedToFloat(uVar16 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
  uVar11 = uVar15;
  DAT_00fd67f8 = uVar16;
  if ((int)(fVar28 * fVar26 * 10.0) == 0) {
    uVar9 = uVar9 ^ uVar9 << 0xb;
    DAT_00fd67f8 = uVar9 ^ uVar16 ^ (uVar9 ^ uVar16 >> 0xb) >> 8;
    fVar28 = (float)VectorSignedToFloat(DAT_00fd67f8 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    fVar28 = (float)VectorSignedToFloat(10 - (int)(fVar28 * fVar26 * -11.0),
                                        (byte)(uVar10 >> 0x16) & 3);
    fVar27 = (fVar28 * DAT_006aa148 + 1.0) * fVar27;
    uVar11 = DAT_00fd67f4;
    uVar9 = uVar15;
    DAT_00fd67f4 = uVar16;
  }
  uVar9 = uVar9 ^ uVar9 << 0xb;
  uVar9 = DAT_00fd67f8 ^ (uVar9 ^ DAT_00fd67f8 >> 0xb) >> 8 ^ uVar9;
  fVar28 = (float)VectorSignedToFloat(uVar9 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
  DAT_00fd67fc = uVar9;
  if ((int)(fVar28 * fVar26 * 15.0) == 0) {
    uVar11 = uVar11 ^ uVar11 << 0xb;
    DAT_00fd67fc = uVar11 ^ uVar9 ^ (uVar11 ^ uVar9 >> 0xb) >> 8;
    fVar28 = (float)VectorSignedToFloat(DAT_00fd67fc & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    fVar28 = (float)VectorSignedToFloat(0xf - (int)(fVar28 * fVar26 * -16.0),
                                        (byte)(uVar10 >> 0x16) & 3);
    fVar27 = (fVar28 * DAT_006aa148 + 1.0) * fVar27;
    uVar11 = DAT_00fd67f4;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = uVar9;
  }
  uVar11 = uVar11 ^ uVar11 << 0xb;
  uVar11 = DAT_00fd67fc ^ (uVar11 ^ DAT_00fd67fc >> 0xb) >> 8 ^ uVar11;
  fVar28 = (float)VectorSignedToFloat(uVar11 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
  DAT_00fd6800 = uVar11;
  if ((int)(fVar28 * fVar26 * 20.0) == 0) {
    uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd6800 = uVar11 ^ (uVar15 ^ uVar11 >> 0xb) >> 8 ^ uVar15;
    fVar28 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
    fVar28 = (float)VectorSignedToFloat(0x14 - (int)(fVar28 * fVar26 * -21.0),
                                        (byte)(uVar10 >> 0x16) & 3);
    fVar27 = (fVar28 * DAT_006aa148 + 1.0) * fVar27;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd67fc = uVar11;
  }
  iVar8 = (int)fVar27;
  uVar15 = DAT_00fd6800;
  uVar9 = DAT_00fd67f4;
  uVar11 = DAT_00fd67f8;
  uVar16 = DAT_00fd67fc;
  while (0 < iVar8) {
    if (DAT_006aa980 < iVar8) {
      iVar19 = (int)((ulonglong)((longlong)iVar8 * (longlong)DAT_006aa97c) >> 0x20);
      local_1a4 = (iVar19 >> 0x12) - (iVar19 >> 0x1f);
      local_19c = uVar15;
      uVar12 = uVar11;
      uVar13 = uVar16;
      if (0x32 < local_1a4) {
        uVar9 = uVar9 ^ uVar9 << 0xb;
        uVar20 = uVar15 ^ (uVar9 ^ uVar15 >> 0xb) >> 8 ^ uVar9;
        fVar27 = (float)VectorSignedToFloat(uVar20 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
        local_19c = uVar20;
        uVar9 = uVar11;
        uVar12 = uVar16;
        uVar13 = uVar15;
        if ((int)(fVar27 * fVar26 * 5.0) == 0) {
          uVar11 = uVar11 ^ uVar11 << 0xb;
          local_19c = uVar20 ^ (uVar11 ^ uVar20 >> 0xb) >> 8 ^ uVar11;
          fVar27 = (float)VectorSignedToFloat(local_19c & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
          iVar19 = 1 - (int)(fVar27 * fVar26 * -3.0);
          if (iVar19 == 0) {
                    /* WARNING: Does not return */
            pcVar2 = (code *)software_udf(0xf9,0x6aa41e);
            (*pcVar2)();
          }
          local_1a4 = FUN_007ffe94(iVar19,local_1a4);
          uVar9 = uVar16;
          uVar12 = uVar15;
          uVar13 = uVar20;
        }
      }
      uVar9 = uVar9 ^ uVar9 << 0xb;
      uVar16 = local_19c ^ (uVar9 ^ local_19c >> 0xb) >> 8 ^ uVar9;
      fVar27 = (float)VectorSignedToFloat(uVar16 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
      uVar15 = uVar16;
      uVar9 = uVar12;
      uVar11 = uVar13;
      DAT_00fd67f4 = uVar12;
      DAT_00fd67f8 = uVar13;
      DAT_00fd67fc = local_19c;
      DAT_00fd6800 = uVar16;
      if ((int)(fVar27 * fVar26 * 5.0) == 0) {
        uVar12 = uVar12 ^ uVar12 << 0xb;
        uVar15 = uVar16 ^ (uVar12 ^ uVar16 >> 0xb) >> 8 ^ uVar12;
        fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
        iVar19 = 1 - (int)(fVar27 * fVar26 * -3.0);
        DAT_00fd67fc = uVar16;
        DAT_00fd6800 = uVar15;
        DAT_00fd67f4 = uVar13;
        DAT_00fd67f8 = local_19c;
        if (iVar19 == 0) {
                    /* WARNING: Does not return */
          pcVar2 = (code *)software_udf(0xf9,0x6aa4ba);
          (*pcVar2)();
        }
        local_1a4 = FUN_007ffe94(iVar19,local_1a4);
        uVar9 = uVar13;
        uVar11 = local_19c;
        local_19c = uVar16;
      }
      uVar16 = local_19c;
      if (0 < local_1a4) {
        iVar8 = iVar8 - local_1a4 * DAT_006aa980;
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x4a,local_1a4,0,0,0);
        iVar7 = local_98;
        uVar15 = DAT_00fd6800;
        uVar9 = DAT_00fd67f4;
        uVar11 = DAT_00fd67f8;
        uVar16 = DAT_00fd67fc;
      }
    }
    else if (iVar8 < 0x2711) {
      if (iVar8 < 0x65) {
        if (iVar8 < 1) break;
        DAT_00fd6800 = uVar15;
        DAT_00fd67f4 = uVar9;
        DAT_00fd67f8 = uVar11;
        DAT_00fd67fc = uVar16;
        local_1a4 = iVar8;
        if (0x32 < iVar8) {
          uVar9 = uVar9 ^ uVar9 << 0xb;
          DAT_00fd6800 = uVar15 ^ (uVar9 ^ uVar15 >> 0xb) >> 8 ^ uVar9;
          fVar27 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
          DAT_00fd67f4 = uVar11;
          DAT_00fd67f8 = uVar16;
          DAT_00fd67fc = uVar15;
          if ((int)(fVar27 * fVar26 * 5.0) == 0) {
            iVar19 = FUN_005c4e6c(&DAT_00fd67f4,1,4);
            if (iVar19 == 0) {
                    /* WARNING: Does not return */
              pcVar2 = (code *)software_udf(0xf9,0x6aa854);
              (*pcVar2)();
            }
            local_1a4 = FUN_007ffe94(iVar19,iVar8);
          }
        }
        uVar15 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
        uVar11 = uVar15 ^ (uVar15 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
        fVar27 = (float)VectorSignedToFloat(uVar11 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
        DAT_00fd67f4 = DAT_00fd67f8;
        uVar15 = DAT_00fd67fc;
        uVar9 = DAT_00fd6800;
        if ((int)(fVar27 * fVar26 * 5.0) == 0) {
          uVar15 = DAT_00fd67f8 ^ DAT_00fd67f8 << 0xb;
          uVar15 = uVar15 ^ (uVar15 ^ uVar11 >> 0xb) >> 8 ^ uVar11;
          fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
          iVar19 = 1 - (int)(fVar27 * fVar26 * -3.0);
          DAT_00fd67f4 = DAT_00fd67fc;
          DAT_00fd67f8 = DAT_00fd6800;
          if (iVar19 == 0) {
                    /* WARNING: Does not return */
            pcVar2 = (code *)software_udf(0xf9,0x6aa8ee);
            DAT_00fd67fc = uVar11;
            DAT_00fd6800 = uVar15;
            (*pcVar2)();
          }
          DAT_00fd67fc = uVar11;
          DAT_00fd6800 = uVar15;
          local_1a4 = FUN_007ffe94(iVar19,local_1a4);
          uVar15 = DAT_00fd67f8;
          uVar9 = DAT_00fd67fc;
          uVar11 = DAT_00fd6800;
        }
        DAT_00fd6800 = uVar11;
        DAT_00fd67fc = uVar9;
        DAT_00fd67f8 = uVar15;
        if (local_1a4 < 1) {
          local_1a4 = 1;
        }
        iVar8 = iVar8 - local_1a4;
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x47,local_1a4,0,0,0);
        iVar7 = local_98;
        uVar15 = DAT_00fd6800;
        uVar9 = DAT_00fd67f4;
        uVar11 = DAT_00fd67f8;
        uVar16 = DAT_00fd67fc;
      }
      else {
        iVar19 = (int)((ulonglong)((longlong)iVar8 * (longlong)DAT_006aa984) >> 0x20);
        local_1a4 = (iVar19 >> 5) - (iVar19 >> 0x1f);
        uVar12 = uVar15;
        uVar13 = uVar11;
        uVar20 = uVar16;
        if (0x32 < local_1a4) {
          uVar9 = uVar9 ^ uVar9 << 0xb;
          uVar21 = uVar15 ^ (uVar9 ^ uVar15 >> 0xb) >> 8 ^ uVar9;
          fVar27 = (float)VectorSignedToFloat(uVar21 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
          uVar12 = uVar21;
          uVar9 = uVar11;
          uVar13 = uVar16;
          uVar20 = uVar15;
          if ((int)(fVar27 * fVar26 * 5.0) == 0) {
            uVar11 = uVar11 ^ uVar11 << 0xb;
            uVar12 = uVar21 ^ (uVar11 ^ uVar21 >> 0xb) >> 8 ^ uVar11;
            fVar27 = (float)VectorSignedToFloat(uVar12 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
            iVar19 = 1 - (int)(fVar27 * fVar26 * -3.0);
            if (iVar19 == 0) {
                    /* WARNING: Does not return */
              pcVar2 = (code *)software_udf(0xf9,0x6aa706);
              (*pcVar2)();
            }
            local_1a4 = FUN_007ffe94(iVar19,local_1a4);
            uVar9 = uVar16;
            uVar13 = uVar15;
            uVar20 = uVar21;
          }
        }
        uVar9 = uVar9 ^ uVar9 << 0xb;
        uVar9 = uVar12 ^ (uVar9 ^ uVar12 >> 0xb) >> 8 ^ uVar9;
        fVar27 = (float)VectorSignedToFloat(uVar9 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
        uVar15 = uVar9;
        DAT_00fd67f4 = uVar13;
        DAT_00fd67f8 = uVar20;
        DAT_00fd67fc = uVar12;
        DAT_00fd6800 = uVar9;
        if ((int)(fVar27 * fVar26 * 5.0) == 0) {
          uVar13 = uVar13 ^ uVar13 << 0xb;
          uVar15 = uVar13 ^ (uVar13 ^ uVar9 >> 0xb) >> 8 ^ uVar9;
          fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
          iVar19 = 1 - (int)(fVar27 * fVar26 * -3.0);
          DAT_00fd67fc = uVar9;
          DAT_00fd6800 = uVar15;
          DAT_00fd67f4 = uVar20;
          DAT_00fd67f8 = uVar12;
          if (iVar19 == 0) {
                    /* WARNING: Does not return */
            pcVar2 = (code *)software_udf(0xf9,0x6aa7a2);
            (*pcVar2)();
          }
          local_1a4 = FUN_007ffe94(iVar19,local_1a4);
          uVar13 = uVar20;
          uVar20 = uVar12;
          uVar12 = uVar9;
        }
        uVar9 = uVar13;
        uVar11 = uVar20;
        uVar16 = uVar12;
        if (0 < local_1a4) {
          iVar8 = iVar8 + local_1a4 * -100;
          FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                       0x48,local_1a4,0,0,0);
          iVar7 = local_98;
          uVar15 = DAT_00fd6800;
          uVar9 = DAT_00fd67f4;
          uVar11 = DAT_00fd67f8;
          uVar16 = DAT_00fd67fc;
        }
      }
    }
    else {
      iVar19 = (int)((ulonglong)((longlong)iVar8 * (longlong)DAT_006aa988) >> 0x20);
      local_1a4 = (iVar19 >> 0xc) - (iVar19 >> 0x1f);
      uVar12 = uVar15;
      uVar13 = uVar11;
      uVar20 = uVar16;
      if (0x32 < local_1a4) {
        uVar9 = uVar9 ^ uVar9 << 0xb;
        uVar21 = uVar15 ^ (uVar9 ^ uVar15 >> 0xb) >> 8 ^ uVar9;
        fVar27 = (float)VectorSignedToFloat(uVar21 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
        uVar12 = uVar21;
        uVar9 = uVar11;
        uVar13 = uVar16;
        uVar20 = uVar15;
        if ((int)(fVar27 * fVar26 * 5.0) == 0) {
          uVar11 = uVar11 ^ uVar11 << 0xb;
          uVar12 = uVar21 ^ (uVar11 ^ uVar21 >> 0xb) >> 8 ^ uVar11;
          fVar27 = (float)VectorSignedToFloat(uVar12 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
          iVar19 = 1 - (int)(fVar27 * fVar26 * -3.0);
          if (iVar19 == 0) {
                    /* WARNING: Does not return */
            pcVar2 = (code *)software_udf(0xf9,0x6aa592);
            (*pcVar2)();
          }
          local_1a4 = FUN_007ffe94(iVar19,local_1a4);
          uVar9 = uVar16;
          uVar13 = uVar15;
          uVar20 = uVar21;
        }
      }
      uVar9 = uVar9 ^ uVar9 << 0xb;
      uVar9 = uVar12 ^ (uVar9 ^ uVar12 >> 0xb) >> 8 ^ uVar9;
      fVar27 = (float)VectorSignedToFloat(uVar9 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
      uVar15 = uVar9;
      DAT_00fd67f4 = uVar13;
      DAT_00fd67f8 = uVar20;
      DAT_00fd67fc = uVar12;
      DAT_00fd6800 = uVar9;
      if ((int)(fVar27 * fVar26 * 5.0) == 0) {
        uVar13 = uVar13 ^ uVar13 << 0xb;
        uVar15 = uVar9 ^ (uVar13 ^ uVar9 >> 0xb) >> 8 ^ uVar13;
        fVar27 = (float)VectorSignedToFloat(uVar15 & 0x7fffffff,(byte)(uVar10 >> 0x16) & 3);
        iVar19 = 1 - (int)(fVar27 * fVar26 * -3.0);
        DAT_00fd67fc = uVar9;
        DAT_00fd6800 = uVar15;
        DAT_00fd67f4 = uVar20;
        DAT_00fd67f8 = uVar12;
        if (iVar19 == 0) {
                    /* WARNING: Does not return */
          pcVar2 = (code *)software_udf(0xf9,0x6aa62c);
          (*pcVar2)();
        }
        local_1a4 = FUN_007ffe94(iVar19,local_1a4);
        uVar13 = uVar20;
        uVar20 = uVar12;
        uVar12 = uVar9;
      }
      uVar9 = uVar13;
      uVar11 = uVar20;
      uVar16 = uVar12;
      if (0 < local_1a4) {
        iVar8 = iVar8 + local_1a4 * -10000;
        FUN_00631590(local_9c,iVar7,*(undefined2 *)(iVar6 + 0x168),*(undefined2 *)(iVar6 + 0x16a),
                     0x49,local_1a4,0,0,0);
        iVar7 = local_98;
        uVar15 = DAT_00fd6800;
        uVar9 = DAT_00fd67f4;
        uVar11 = DAT_00fd67f8;
        uVar16 = DAT_00fd67fc;
      }
    }
  }
LAB_006aa94a:
  FUN_007ffb98();
  return;
}


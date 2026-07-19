
void FUN_00782ae4(void)

{
  byte bVar1;
  bool bVar2;
  float fVar3;
  undefined4 uVar4;
  uint uVar5;
  uint uVar6;
  uint uVar7;
  uint uVar8;
  int iVar9;
  int iVar10;
  int iVar11;
  int iVar12;
  uint in_fpscr;
  uint uVar13;
  float fVar15;
  float fVar16;
  float fVar17;
  float fVar18;
  float fVar19;
  undefined8 uVar14;
  float fVar20;
  float fVar21;
  float fVar22;
  undefined8 uVar23;
  int local_54;
  int local_44;
  int local_40;
  undefined4 local_3c;
  undefined4 local_38;
  
  uVar23 = FUN_007ffb80();
  fVar3 = DAT_00782c08;
  if (DAT_010338d3 != '\0') {
    iVar9 = (int)((ulonglong)((longlong)DAT_0102a77c * (longlong)DAT_00782c0c) >> 0x20);
    iVar9 = iVar9 - (iVar9 >> 0x1f);
    iVar10 = DAT_0102a77c + iVar9 * -3;
    fVar15 = (float)VectorSignedToFloat((int)DAT_00902ea0,(byte)(in_fpscr >> 0x16) & 3);
    fVar20 = (float)VectorSignedToFloat(iVar10 * 0x37,(byte)(in_fpscr >> 0x16) & 3);
    fVar21 = (float)VectorSignedToFloat(iVar9 + 1,(byte)(in_fpscr >> 0x16) & 3);
    fVar21 = (fVar15 * DAT_00782c04 * DAT_00782c00 - fVar20) / fVar21;
    if (iVar10 == 0) {
      iVar9 = (int)DAT_00902c30;
      if (iVar9 < 0) {
        uVar6 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
        DAT_00fd67f4 = DAT_00fd67f8;
        DAT_00fd67f8 = DAT_00fd67fc;
        uVar6 = DAT_00fd6800 ^ (uVar6 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar6;
        DAT_00fd67fc = DAT_00fd6800;
        fVar15 = (float)VectorSignedToFloat(uVar6 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
        if ((int)(fVar15 * DAT_00782c08 * 2.0) == 0) {
          iVar9 = 0xdd;
        }
        else {
          iVar9 = 0x6b;
        }
        DAT_00902c30 = (short)iVar9;
        DAT_00fd6800 = uVar6;
      }
      if (iVar9 == 0x6b) {
        uVar4 = 0xc;
        fVar21 = fVar21 * DAT_00782bfc;
      }
      else {
        uVar4 = 0x27;
        fVar21 = fVar21 * DAT_00782bfc;
      }
    }
    else if (iVar10 == 1) {
      iVar9 = (int)DAT_00902c34;
      if (iVar9 < 0) {
        iVar9 = FUN_00608330(&DAT_00fd67f4,2);
        if (iVar9 == 0) {
          iVar9 = 0xde;
        }
        else {
          iVar9 = 0x6c;
        }
        DAT_00902c34 = (short)iVar9;
      }
      if (iVar9 == 0x6c) {
        uVar4 = 0xd;
      }
      else {
        uVar4 = 0x28;
      }
    }
    else {
      iVar9 = (int)DAT_00902c38;
      if (iVar9 < 0) {
        iVar9 = FUN_00608330(&DAT_00fd67f4,2);
        if (iVar9 == 0) {
          iVar9 = 0xdf;
        }
        else {
          iVar9 = 0x6f;
        }
        DAT_00902c38 = (short)iVar9;
      }
      if (iVar9 == 0x6f) {
        uVar4 = 0xe;
      }
      else {
        uVar4 = 0x29;
      }
    }
    FUN_00646f20(uVar4,0x32,0xff,0x82,0xffffffff);
    iVar11 = 0;
    uVar6 = in_fpscr & 0xfffffff | (uint)(fVar21 < 0.0) << 0x1f | (uint)(fVar21 == 0.0) << 0x1e;
    uVar13 = uVar6 | (uint)NAN(fVar21) << 0x1c;
    bVar1 = (byte)(uVar6 >> 0x18);
    local_54 = 0;
    if (!(bool)(bVar1 >> 6 & 1) && bVar1 >> 7 == ((byte)(uVar13 >> 0x1c) & 1)) {
      fVar15 = (float)VectorSignedToFloat(-iVar10 + 4,(byte)(uVar13 >> 0x16) & 3);
      fVar20 = (float)VectorSignedToFloat(-iVar10 + 2,(byte)(uVar13 >> 0x16) & 3);
      do {
        uVar6 = DAT_0103089c;
        uVar7 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
        uVar7 = DAT_00fd6800 ^ (uVar7 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar7;
        fVar16 = (float)VectorSignedToFloat(uVar7 & 0x7fffffff,(byte)(uVar13 >> 0x16) & 3);
        fVar17 = (float)VectorSignedToFloat(DAT_00902ea0 + -200,(byte)(uVar13 >> 0x16) & 3);
        iVar10 = DAT_01033960;
        if ((iVar9 != 0x6c) && (iVar10 = DAT_00902ebc, iVar9 == 0x6f)) {
          iVar10 = (int)((ulonglong)
                         ((longlong)((int)DAT_00902ea4 + DAT_01033960 * 2) * (longlong)DAT_00783034)
                        >> 0x20);
          iVar10 = iVar10 - (iVar10 >> 0x1f);
        }
        uVar5 = DAT_00fd67f8 ^ DAT_00fd67f8 << 0xb;
        DAT_00fd67f4 = DAT_00fd67fc;
        DAT_00fd67f8 = DAT_00fd6800;
        DAT_00fd6800 = uVar7 ^ (uVar5 ^ uVar7 >> 0xb) >> 8 ^ uVar5;
        uVar5 = DAT_01030898 ^ DAT_01030898 << 0xb;
        uVar5 = uVar5 ^ (uVar5 ^ DAT_010308a4 >> 0xb) >> 8 ^ DAT_010308a4;
        DAT_01030898 = DAT_010308a0;
        DAT_0103089c = DAT_010308a4;
        uVar6 = uVar6 ^ uVar6 << 0xb;
        DAT_010308a4 = uVar6 ^ (uVar6 ^ uVar5 >> 0xb) >> 8 ^ uVar5;
        fVar18 = (float)VectorSignedToFloat(DAT_010308a4 & 0x7fffffff,(byte)(uVar13 >> 0x16) & 3);
        fVar19 = (float)VectorSignedToFloat(uVar5 & 0x7fffffff,(byte)(uVar13 >> 0x16) & 3);
        fVar22 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar13 >> 0x16) & 3);
        uVar14 = VectorSignedToFloat((int)(fVar19 * fVar3 * fVar20) + 5,(byte)(uVar13 >> 0x16) & 3);
        fVar19 = (float)VectorSignedToFloat((DAT_00902ea4 - iVar10) + -0x96,
                                            (byte)(uVar13 >> 0x16) & 3);
        DAT_00fd67fc = uVar7;
        DAT_010308a0 = uVar5;
        iVar10 = FUN_0077591c((int)uVar14,(int)(fVar16 * fVar3 * fVar17) + 100,
                              (int)(fVar19 * fVar22 * fVar3) + iVar10,
                              (int)(fVar18 * fVar3 * fVar15) + 3,iVar9);
        if (local_54 < iVar10) {
          local_54 = iVar10;
        }
        iVar11 = iVar11 + 1;
        fVar16 = (float)VectorSignedToFloat(iVar11,(byte)(uVar13 >> 0x16) & 3);
        uVar13 = uVar13 & 0xfffffff | (uint)(fVar21 <= fVar16) << 0x1d;
      } while ((byte)(uVar13 >> 0x1d) == 0);
    }
    uVar6 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    uVar6 = DAT_00fd6800 ^ (uVar6 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar6;
    fVar15 = (float)VectorSignedToFloat(uVar6 & 0x7fffffff,(byte)(uVar13 >> 0x16) & 3);
    iVar9 = (int)(fVar15 * fVar3 * 3.0);
    uVar7 = DAT_00fd6800;
    DAT_00fd67f4 = DAT_00fd67f8;
    if (iVar9 != 2) {
      fVar15 = (float)VectorSignedToFloat(DAT_00902ea0 + -200,(byte)(uVar13 >> 0x16) & 3);
      fVar20 = (float)VectorSignedToFloat(((int)DAT_00902ea4 - (DAT_01033960 + 0x32)) + -300,
                                          (byte)(uVar13 >> 0x16) & 3);
      uVar5 = DAT_00fd67f8;
      uVar8 = DAT_00fd67fc;
      do {
        DAT_00fd67f4 = uVar7;
        DAT_00fd67f8 = uVar6;
        uVar5 = uVar5 ^ uVar5 << 0xb;
        DAT_00fd67fc = uVar5 ^ (uVar5 ^ DAT_00fd67f8 >> 0xb) >> 8 ^ DAT_00fd67f8;
        uVar8 = uVar8 ^ uVar8 << 0xb;
        fVar21 = (float)VectorSignedToFloat(DAT_00fd67fc & 0x7fffffff,(byte)(uVar13 >> 0x16) & 3);
        iVar12 = (int)(fVar21 * fVar3 * fVar15) + 100;
        DAT_00fd6800 = uVar8 ^ (uVar8 ^ DAT_00fd67fc >> 0xb) >> 8 ^ DAT_00fd67fc;
        fVar21 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar13 >> 0x16) & 3);
        iVar11 = (int)(fVar21 * fVar3 * fVar20) + DAT_01033960 + 0x32;
        iVar10 = (DAT_00902934 * iVar12 + iVar11) * 0xe + DAT_00902928;
        if ((*(short *)(iVar10 + 6) == 1) && ((*(byte *)(iVar10 + 1) & 1) != 0)) {
          bVar2 = true;
        }
        else {
          bVar2 = false;
        }
        uVar6 = DAT_00fd6800;
        uVar7 = DAT_00fd67fc;
        uVar5 = DAT_00fd67f4;
        uVar8 = DAT_00fd67f8;
      } while (!bVar2);
      if (iVar9 == 0) {
        if (DAT_0102a787 == '\0') {
          *(undefined2 *)(iVar10 + 6) = 0x19;
        }
        else {
          *(undefined2 *)(iVar10 + 6) = 0xcb;
        }
      }
      else {
        *(undefined2 *)(iVar10 + 6) = 0x75;
      }
      FUN_00649a94(iVar12,iVar11,0,0);
      uVar6 = DAT_00fd6800;
      uVar7 = DAT_00fd67fc;
      DAT_00fd67fc = DAT_00fd67f8;
    }
    uVar5 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd6800 = uVar5 ^ (uVar5 ^ uVar6 >> 0xb) >> 8 ^ uVar6;
    fVar15 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar13 >> 0x16) & 3);
    iVar9 = 1 - (int)(fVar15 * fVar3 * -2.0);
    local_44 = (int)uVar23 << 4;
    local_40 = (int)((ulonglong)uVar23 >> 0x20) << 4;
    local_3c = 0x10;
    local_38 = 0x10;
    DAT_00fd67f4 = DAT_00fd67fc;
    DAT_00fd67f8 = uVar7;
    DAT_00fd67fc = uVar6;
    if (0 < iVar9) {
      do {
        uVar4 = FUN_006c314c(&local_44);
        FUN_00685904(uVar4,0x52);
        iVar9 = iVar9 + -1;
      } while (iVar9 != 0);
    }
    DAT_0102a77c = DAT_0102a77c + 1;
  }
  FUN_007ffb98();
  return;
}


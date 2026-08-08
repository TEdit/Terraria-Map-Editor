
void FUN_006b9ee8(int param_1)

{
  bool bVar1;
  byte bVar2;
  byte bVar3;
  ushort uVar4;
  char cVar5;
  bool bVar6;
  bool bVar7;
  int *piVar8;
  int iVar9;
  uint uVar10;
  uint uVar11;
  int iVar12;
  int iVar13;
  int iVar14;
  int iVar15;
  int iVar16;
  int iVar17;
  uint in_fpscr;
  uint uVar18;
  uint uVar19;
  float fVar20;
  float fVar21;
  undefined4 uVar22;
  undefined4 uVar23;
  float fVar24;
  float fVar25;
  float fVar26;
  float fVar27;
  float fVar28;
  int *local_4c;
  
  uVar22 = DAT_006b9ffc;
  fVar25 = DAT_006b9ff8;
  if (((DAT_010299a0 == '\0') && (DAT_010703e1 == '\0')) && (DAT_010703e0 != '\0')) {
    bVar7 = false;
  }
  else {
    bVar7 = true;
  }
  iVar15 = *(int *)(param_1 + 0x104);
  local_4c = &DAT_0107034c;
  if ((((iVar15 == 0x2e) || (iVar15 == 0x94)) ||
      ((iVar15 == 0x95 || ((iVar15 == 0xe6 || (iVar15 == 299)))))) ||
     ((iVar15 == 300 || (iVar15 == 0x12f)))) {
    if (*(char *)(param_1 + 0x175) == '\x04') {
      FUN_0065b1e4(param_1,1);
      fVar21 = *(float *)(param_1 + 0x138);
      fVar20 = *(float *)((&DAT_0107034c)[*(byte *)(param_1 + 0x175)] + 0x110);
      uVar18 = in_fpscr & 0xfffffff;
      in_fpscr = uVar18 | (uint)(fVar20 <= fVar21) << 0x1d;
      if ((byte)(in_fpscr >> 0x1d) == 0) {
        *(undefined1 *)(param_1 + 0x171) = 1;
        *(undefined1 *)(param_1 + 0x1e1) = 1;
      }
      else {
        uVar18 = uVar18 | (uint)(fVar21 < fVar20) << 0x1f | (uint)(fVar21 == fVar20) << 0x1e;
        in_fpscr = uVar18 | (uint)(NAN(fVar21) || NAN(fVar20)) << 0x1c;
        bVar2 = (byte)(uVar18 >> 0x18);
        if (!(bool)(bVar2 >> 6 & 1) && bVar2 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
          *(undefined1 *)(param_1 + 0x171) = 0xff;
          *(undefined1 *)(param_1 + 0x1e1) = 0xff;
        }
      }
      if (*(short *)(param_1 + 0x1f2) == -1) {
        *(short *)(param_1 + 0x1f2) =
             (short)(*(int *)(param_1 + 0x148) + (*(int *)(param_1 + 0x150) >> 1) >> 4);
      }
    }
    if (((*(int *)(param_1 + 0x104) == 0x94) || (*(int *)(param_1 + 0x104) == 0x95)) &&
       (*(int *)(param_1 + 0x188) == 0)) {
      uVar18 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
      DAT_00fd67f4 = DAT_00fd67f8;
      DAT_00fd67f8 = DAT_00fd67fc;
      uVar18 = uVar18 ^ (uVar18 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
      DAT_00fd67fc = DAT_00fd6800;
      fVar21 = (float)VectorSignedToFloat(uVar18 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      DAT_00fd6800 = uVar18;
      *(int *)(param_1 + 0x188) = 1 - (int)(fVar21 * fVar25 * -4.0);
    }
  }
  else if (iVar15 == 0x6b) {
    DAT_00fe1f7e = 1;
  }
  else if (iVar15 == 0x6c) {
    DAT_00fe1f7f = 1;
  }
  else if (iVar15 == 0x7c) {
    DAT_00fe1f82 = 1;
  }
  else if (((iVar15 == 0x8e) && (iVar15 = FUN_0073542c(), iVar15 == 0)) && (DAT_00fe1ec4 != 1)) {
    FUN_006b4dbc(param_1,9999,0,0,0);
    FUN_00647074((int)DAT_006b9ff0,(int)*(short *)(param_1 + 0x1e6),9999,0,0);
  }
  uVar23 = DAT_006ba35c;
  fVar21 = DAT_006ba358;
  iVar15 = *(int *)(param_1 + 0x148) + (*(int *)(param_1 + 0x150) >> 1) >> 4;
  iVar16 = (int)(*(int *)(param_1 + 0x14c) + (uint)*(ushort *)(param_1 + 0x16a) + 1) >> 4;
  bVar1 = false;
  *(undefined1 *)(param_1 + 0x172) = 0xff;
  *(byte *)(param_1 + 0x171) = *(byte *)(param_1 + 0x171) | 1;
  iVar17 = 4;
  piVar8 = &DAT_0107034c;
  do {
    if ((*(char *)(*piVar8 + 0x5cc5) != '\0') &&
       (*(short *)(*piVar8 + 0x8df0) == *(short *)(param_1 + 0x1e6))) {
      bVar1 = true;
      in_fpscr = in_fpscr & 0xfffffff | (uint)(*(float *)(param_1 + 0x178) == 0.0) << 0x1e;
      if ((byte)(in_fpscr >> 0x1e) == 0) {
        *(undefined1 *)(param_1 + 0x10c) = 1;
      }
      *(undefined4 *)(param_1 + 0x178) = uVar22;
      *(undefined4 *)(param_1 + 0x17c) = uVar23;
      *(float *)(param_1 + 0x180) = fVar21;
      if (*(int *)(*piVar8 + 0x100) + (*(int *)(*piVar8 + 0x108) >> 1) <
          *(int *)(param_1 + 0x148) + (*(int *)(param_1 + 0x150) >> 1)) {
        *(undefined1 *)(param_1 + 0x171) = 0xff;
      }
      else {
        *(undefined1 *)(param_1 + 0x171) = 1;
      }
    }
    iVar17 = iVar17 + -1;
    piVar8 = piVar8 + 1;
  } while (iVar17 != 0);
  fVar20 = *(float *)(param_1 + 0x184);
  uVar11 = in_fpscr & 0xfffffff | (uint)(fVar20 < 0.0) << 0x1f | (uint)(fVar20 == 0.0) << 0x1e;
  uVar18 = uVar11 | (uint)NAN(fVar20) << 0x1c;
  bVar2 = (byte)(uVar11 >> 0x18);
  if (((bool)(bVar2 >> 6 & 1) || bVar2 >> 7 != ((byte)(uVar18 >> 0x1c) & 1)) ||
     (*(char *)(param_1 + 0x100) == '\0')) {
LAB_006ba1d4:
    if ((*(int *)(param_1 + 0x104) == 0x25) && (DAT_00fe1ec4 != 1)) {
      *(undefined1 *)(param_1 + 0x119) = 0;
      *(undefined2 *)(param_1 + 0x1f2) = DAT_01033968;
      *(undefined2 *)(param_1 + 500) = DAT_01033964;
      if (DAT_00fe1f7b != '\0') {
        *(undefined4 *)(param_1 + 0x184) = 0x3f800000;
        *(undefined1 *)(param_1 + 0x10c) = 1;
      }
    }
  }
  else {
    *(undefined4 *)(param_1 + 0x1b4) = 0xffffffff;
    FUN_00697e24(0x41200000,param_1,0);
    *(undefined1 *)(param_1 + 0x100) = 0;
    if (*(int *)(param_1 + 0x104) == 0x25) {
      FUN_007a8384(0xf,*(undefined4 *)(param_1 + 0x148),*(undefined4 *)(param_1 + 0x14c),0);
      goto LAB_006ba1d4;
    }
  }
  fVar20 = DAT_006ba354;
  iVar17 = (int)*(short *)(param_1 + 500);
  if (DAT_00fe1ec4 != 1) {
    if (0 < iVar17) {
      iVar9 = FUN_007521fc((int)*(short *)(param_1 + 0x1f2),iVar17);
      while ((iVar9 == 0 && (iVar17 < DAT_00902ea4 + -0x14))) {
        iVar17 = iVar17 + 1;
        iVar9 = FUN_007521fc((int)*(short *)(param_1 + 0x1f2),iVar17);
      }
    }
    if (((((DAT_00fe1ec4 != 1) && (*(char *)(param_1 + 0x118) != '\0')) &&
         (*(char *)(param_1 + 0x119) == '\0')) &&
        ((iVar15 != *(short *)(param_1 + 0x1f2) || (iVar16 != iVar17)))) &&
       ((bVar7 || ((DAT_010703e0 == '\0' ||
                   (((&DAT_010262d0)
                     [(uint)*(ushort *)((DAT_00902934 * iVar15 + iVar16) * 0xe + DAT_00902928 + 6) *
                      4] & 0x2000) != 0)))))) {
      iVar12 = *(int *)(param_1 + 0x148) + (*(int *)(param_1 + 0x150) >> 1);
      iVar13 = *(int *)(param_1 + 0x14c) + (*(int *)(param_1 + 0x154) >> 1);
      iVar9 = 0;
      piVar8 = &DAT_0107034c;
      do {
        iVar14 = *piVar8;
        if (*(char *)(iVar14 + 0x5cc5) != '\0') {
          if ((((*(int *)(iVar14 + 0x100) < iVar12 + 0x3fe) &&
               (iVar12 + -0x3fe < *(int *)(iVar14 + 0x108) + *(int *)(iVar14 + 0x100))) &&
              (*(int *)(iVar14 + 0x104) < iVar13 + 0x23e)) &&
             (iVar13 + -0x23e < *(int *)(iVar14 + 0x10c) + *(int *)(iVar14 + 0x104))) {
            bVar6 = true;
          }
          else {
            bVar6 = false;
          }
          if (bVar6) {
            bVar6 = false;
            goto LAB_006ba36e;
          }
        }
        iVar9 = iVar9 + 1;
        piVar8 = piVar8 + 1;
      } while (iVar9 < 4);
      bVar6 = true;
LAB_006ba36e:
      if (bVar6) {
        iVar12 = 0;
        iVar9 = *(short *)(param_1 + 0x1f2) * 0x10;
        do {
          iVar13 = *local_4c;
          if (*(char *)(iVar13 + 0x5cc5) != '\0') {
            if (((*(int *)(iVar13 + 0x100) < iVar9 + 0x406) &&
                (iVar9 + -0x3f6 < *(int *)(iVar13 + 0x108) + *(int *)(iVar13 + 0x100))) &&
               ((*(int *)(iVar13 + 0x104) < iVar17 * 0x10 + 0x246 &&
                (iVar17 * 0x10 + -0x236 < *(int *)(iVar13 + 0x10c) + *(int *)(iVar13 + 0x104))))) {
              bVar6 = true;
            }
            else {
              bVar6 = false;
            }
            if (bVar6) {
              bVar6 = false;
              break;
            }
          }
          iVar12 = iVar12 + 1;
          bVar6 = true;
          local_4c = local_4c + 1;
        } while (iVar12 < 4);
        if (bVar6) {
          if ((*(int *)(param_1 + 0x104) == 0x25) ||
             (iVar9 = FUN_00609344(*(short *)(param_1 + 0x1f2) + -1,*(short *)(param_1 + 0x1f2) + 1,
                                   iVar17 + -3,iVar17 + -1), iVar9 == 0)) {
            *(undefined4 *)(param_1 + 0x140) = DAT_00900110;
            *(undefined4 *)(param_1 + 0x144) = DAT_00900114;
            iVar9 = (*(short *)(param_1 + 0x1f2) * 0x10 - (uint)(*(ushort *)(param_1 + 0x168) >> 1))
                    + 8;
            fVar24 = (float)VectorSignedToFloat(iVar17 * 0x10 - (uint)*(ushort *)(param_1 + 0x16a),
                                                (byte)(uVar18 >> 0x16) & 3);
            *(int *)(param_1 + 0x148) = iVar9;
            uVar23 = VectorSignedToFloat(iVar9,(byte)(uVar18 >> 0x16) & 3);
            *(undefined1 *)(param_1 + 0x10c) = 1;
            *(float *)(param_1 + 0x13c) = fVar24 - fVar20;
            *(undefined4 *)(param_1 + 0x138) = uVar23;
            *(int *)(param_1 + 0x14c) = (int)(fVar24 - fVar20);
          }
          else {
            *(undefined1 *)(param_1 + 0x119) = 1;
            FUN_0076098c((int)*(short *)(param_1 + 0x1e6));
          }
        }
      }
    }
  }
  uVar18 = uVar18 & 0xfffffff;
  if (*(float *)(param_1 + 0x178) != 0.0) {
    uVar11 = uVar18 | (uint)(*(float *)(param_1 + 0x178) == 1.0) << 0x1e;
    if ((byte)(uVar11 >> 0x1e) == 0) {
      return;
    }
    if (DAT_00fe1ec4 != 1) {
      if ((((bVar7) && (iVar15 == *(short *)(param_1 + 0x1f2))) &&
          (iVar16 == *(short *)(param_1 + 500))) &&
         (((((iVar17 = *(int *)(param_1 + 0x104), iVar17 != 0x2e && (iVar17 != 0x94)) &&
            ((iVar17 != 0x95 && ((iVar17 != 0xe6 && (iVar17 != 299)))))) && (iVar17 != 300)) &&
          (iVar17 != 0x12f)))) {
        *(undefined4 *)(param_1 + 0x178) = uVar22;
        iVar15 = FUN_00608330(&DAT_00fd67f4,200);
        *(undefined4 *)(param_1 + 0x180) = DAT_006ba9e4;
        uVar22 = VectorSignedToFloat(iVar15 + 200,(byte)(uVar11 >> 0x16) & 3);
        *(undefined1 *)(param_1 + 0x10c) = 1;
        *(undefined4 *)(param_1 + 0x17c) = uVar22;
        return;
      }
      if (((*(char *)(param_1 + 0x119) == '\0') &&
          (((&DAT_010262d0)
            [(uint)*(ushort *)((DAT_00902934 * iVar15 + iVar16) * 0xe + DAT_00902928 + 6) * 4] &
           0x2000) == 0)) &&
         ((iVar16 = (int)*(short *)(param_1 + 0x1f2), iVar15 < iVar16 + -0x23 ||
          (iVar16 + 0x23 < iVar15)))) {
        if ((*(int *)(param_1 + 0x148) < iVar16 * 0x10) && (*(char *)(param_1 + 0x171) < '\0')) {
          *(float *)(param_1 + 0x17c) = *(float *)(param_1 + 0x17c) - 5.0;
        }
        else if ((iVar16 * 0x10 < *(int *)(param_1 + 0x148)) && ('\0' < *(char *)(param_1 + 0x171)))
        {
          *(float *)(param_1 + 0x17c) = *(float *)(param_1 + 0x17c) - 5.0;
        }
      }
    }
    fVar20 = *(float *)(param_1 + 0x17c) - 1.0;
    uVar18 = uVar18 | (uint)(fVar20 == 0.0) << 0x1e | (uint)(0.0 <= fVar20) << 0x1d;
    *(float *)(param_1 + 0x17c) = fVar20;
    bVar2 = (byte)(uVar18 >> 0x18);
    if (!(bool)(bVar2 >> 5 & 1) || (bool)(bVar2 >> 6)) {
      *(undefined4 *)(param_1 + 0x178) = uVar22;
      uVar11 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
      DAT_00fd67f4 = DAT_00fd67f8;
      DAT_00fd67f8 = DAT_00fd67fc;
      uVar11 = uVar11 ^ (uVar11 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
      DAT_00fd67fc = DAT_00fd6800;
      fVar20 = (float)VectorSignedToFloat(uVar11 & 0x7fffffff,(byte)(uVar18 >> 0x16) & 3);
      iVar16 = *(int *)(param_1 + 0x104);
      uVar23 = VectorSignedToFloat(300 - (int)(fVar20 * fVar25 * DAT_006ba9e0),
                                   (byte)(uVar18 >> 0x16) & 3);
      DAT_00fd6800 = uVar11;
      *(undefined4 *)(param_1 + 0x17c) = uVar23;
      uVar11 = DAT_00fd67f4;
      if ((((iVar16 == 0x2e) || (iVar16 == 0x94)) || (iVar16 == 0x95)) ||
         (((iVar16 == 0xe6 || (iVar16 == 299)) || ((iVar16 == 300 || (iVar16 == 0x12f)))))) {
        DAT_00fd67f4 = DAT_00fd67f8;
        uVar11 = uVar11 ^ uVar11 << 0xb;
        DAT_00fd67f8 = DAT_00fd67fc;
        uVar11 = uVar11 ^ (uVar11 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
        DAT_00fd67fc = DAT_00fd6800;
        fVar20 = (float)VectorSignedToFloat(uVar11 & 0x7fffffff,(byte)(uVar18 >> 0x16) & 3);
        fVar21 = (float)VectorSignedToFloat((int)(fVar20 * fVar25 * fVar21),
                                            (byte)(uVar18 >> 0x16) & 3);
        DAT_00fd6800 = uVar11;
        *(float *)(param_1 + 0x17c) = *(float *)(param_1 + 0x17c) - fVar21;
      }
      uVar23 = DAT_006bad50;
      *(undefined1 *)(param_1 + 0x10c) = 1;
      *(undefined4 *)(param_1 + 0x180) = uVar23;
    }
    if (*(char *)(param_1 + 0x11b) != '\0') {
      iVar16 = (int)*(short *)(param_1 + 0x1fa);
      iVar17 = *(int *)(param_1 + 0x148) + (*(int *)(param_1 + 0x150) >> 1) >> 4;
      if ((iVar16 + 2 < iVar17) || (iVar17 < iVar16 + -2)) {
        iVar16 = FUN_00772624(iVar16,(int)*(short *)(param_1 + 0x1fc),0);
        if (iVar16 == 0) {
          iVar16 = *(int *)(param_1 + 0x14c) + (*(int *)(param_1 + 0x154) >> 1) >> 4;
          if ((((*(short *)(param_1 + 0x1fa) + 4 < iVar17) ||
               (iVar17 < *(short *)(param_1 + 0x1fa) + -4)) ||
              (*(short *)(param_1 + 0x1fc) + 4 < iVar16)) ||
             (iVar16 < *(short *)(param_1 + 0x1fc) + -4)) {
            *(undefined1 *)(param_1 + 0x11b) = 0;
          }
        }
        else {
          *(undefined1 *)(param_1 + 0x11b) = 0;
          FUN_006494f4((int)*(short *)(param_1 + 0x1fa),(int)*(short *)(param_1 + 0x1fc));
        }
      }
    }
    fVar21 = 1.0;
    if (*(int *)(param_1 + 0x104) == 299) {
      fVar21 = 1.5;
    }
    fVar20 = DAT_006bad4c;
    if (*(int *)(param_1 + 0x104) == 300) {
      fVar21 = 2.0;
      fVar20 = 1.0;
    }
    fVar24 = *(float *)(param_1 + 0x140);
    fVar26 = -fVar21;
    uVar18 = uVar18 & 0xfffffff;
    uVar11 = uVar18;
    if ((fVar24 < fVar26) ||
       (uVar11 = uVar18 | (uint)(fVar24 < fVar21) << 0x1f,
       fVar24 != fVar21 && SUB41(uVar11 >> 0x1f,0) == (NAN(fVar24) || NAN(fVar21)))) {
      uVar11 = uVar11 & 0xfffffff;
      if (*(float *)(param_1 + 0x144) == 0.0) {
        fVar21 = *(float *)(param_1 + 0x144) * DAT_006bad48;
        *(float *)(param_1 + 0x140) = *(float *)(param_1 + 0x140) * DAT_006bad48;
        *(float *)(param_1 + 0x144) = fVar21;
      }
    }
    else if ((fVar21 <= fVar24) || (*(char *)(param_1 + 0x171) != '\x01')) {
      uVar11 = uVar18 | (uint)(fVar24 < fVar26) << 0x1f;
      if ((fVar24 != fVar26 && SUB41(uVar11 >> 0x1f,0) == (NAN(fVar24) || NAN(fVar26))) &&
         (*(char *)(param_1 + 0x171) == -1)) {
        fVar24 = fVar24 - fVar20;
        uVar11 = uVar18 | (uint)(fVar24 < fVar21) << 0x1f;
        *(float *)(param_1 + 0x140) = fVar24;
        if (fVar24 != fVar21 && SUB41(uVar11 >> 0x1f,0) == (NAN(fVar24) || NAN(fVar21))) {
          *(float *)(param_1 + 0x140) = fVar21;
        }
      }
    }
    else {
      fVar24 = fVar24 + fVar20;
      uVar11 = uVar18 | (uint)(fVar24 < fVar21) << 0x1f;
      *(float *)(param_1 + 0x140) = fVar24;
      if (fVar24 != fVar21 && SUB41(uVar11 >> 0x1f,0) == (NAN(fVar24) || NAN(fVar21))) {
        *(float *)(param_1 + 0x140) = fVar21;
      }
    }
    fVar21 = (float)VectorSignedToFloat((*(short *)(param_1 + 500) + -2) * 0x10,
                                        (byte)(uVar11 >> 0x16) & 3);
    uVar11 = uVar11 & 0xfffffff;
    bVar1 = fVar21 <= *(float *)(param_1 + 0x13c);
    cVar5 = *(char *)(param_1 + 0x171);
    if (((cVar5 == '\x01') &&
        (*(short *)(param_1 + 0x1f2) * 0x10 <
         *(int *)(param_1 + 0x148) + (*(int *)(param_1 + 0x150) >> 1))) ||
       ((cVar5 == -1 &&
        (*(int *)(param_1 + 0x148) + (*(int *)(param_1 + 0x150) >> 1) <
         *(short *)(param_1 + 0x1f2) * 0x10)))) {
      bVar1 = true;
    }
    fVar21 = *(float *)(param_1 + 0x144);
    uVar18 = uVar11 | (uint)(fVar21 < 0.0) << 0x1f;
    if (SUB41(uVar18 >> 0x1f,0) == NAN(fVar21)) {
      fVar20 = *(float *)(param_1 + 0x140);
      iVar16 = 0;
      if (fVar20 < 0.0) {
        iVar16 = -1;
      }
      uVar18 = uVar11 | (uint)(fVar20 < 0.0) << 0x1f;
      if (fVar20 != 0.0 && SUB41(uVar18 >> 0x1f,0) == NAN(fVar20)) {
        iVar16 = 1;
      }
      uVar4 = *(ushort *)(param_1 + 0x168);
      fVar26 = (float)VectorSignedToFloat((uint)(uVar4 >> 1),(byte)(uVar18 >> 0x16) & 3);
      fVar20 = fVar20 + *(float *)(param_1 + 0x138);
      fVar24 = (float)VectorSignedToFloat(((uVar4 >> 1) + 1) * iVar16,(byte)(uVar18 >> 0x16) & 3);
      fVar28 = (float)VectorSignedToFloat((uint)*(ushort *)(param_1 + 0x16a),
                                          (byte)(uVar18 >> 0x16) & 3);
      iVar17 = (int)((fVar26 + fVar20 + fVar24) * DAT_006bad44);
      fVar27 = fVar28 + *(float *)(param_1 + 0x13c);
      fVar26 = (float)VectorSignedToFloat((uint)uVar4,(byte)(uVar18 >> 0x16) & 3);
      fVar24 = (float)VectorSignedToFloat(iVar17 * 0x10,(byte)(uVar18 >> 0x16) & 3);
      iVar9 = (int)((fVar27 - 1.0) * DAT_006bad44);
      uVar18 = uVar11;
      if (fVar24 < fVar26 + fVar20) {
        fVar24 = (float)VectorSignedToFloat(iVar17 * 0x10 + 0x10,(byte)(uVar11 >> 0x16) & 3);
        uVar18 = uVar11 | (uint)(fVar24 < fVar20) << 0x1f;
        if (fVar24 != fVar20 && SUB41(uVar18 >> 0x1f,0) == (NAN(fVar24) || NAN(fVar20))) {
          iVar12 = DAT_00902934 * iVar17 + iVar9;
          iVar13 = iVar12 * 0xe + DAT_00902928;
          if (((*(byte *)(iVar13 + 1) & 1) == 0) || ((*(byte *)(iVar13 + 1) & 2) != 0)) {
            bVar6 = false;
          }
          else {
            bVar6 = true;
          }
          if (((bVar6) && ((*(byte *)(iVar13 + 1) & 0x18) == 0)) &&
             (bVar2 = *(byte *)(iVar13 + -0xd), (bVar2 & 0x18) == 0)) {
            uVar19 = (uint)*(ushort *)(iVar13 + 6);
            uVar10 = (&DAT_010262d0)[uVar19 * 4];
            if (((uVar10 & 1) == 0) || ((uVar10 & 2) != 0)) {
              if ((bVar1) && (((uVar10 & 2) != 0 && (*(short *)(iVar13 + 0xc) == 0)))) {
                if (((&DAT_010262d0)[(uint)*(ushort *)(iVar13 + -8) * 4] & 1) != 0) {
                  bVar3 = *(byte *)(iVar12 * 0xe + DAT_00902928 + -0xd);
                  if (((bVar3 & 1) == 0) || ((bVar3 & 2) != 0)) {
                    bVar1 = false;
                  }
                  else {
                    bVar1 = true;
                  }
                  if (bVar1) goto LAB_006bade0;
                }
                if (((uVar19 != 0x10) && (uVar19 != 0x12)) && (uVar19 != 0x86)) goto LAB_006bae10;
              }
              goto LAB_006bade0;
            }
LAB_006bae10:
            bVar3 = *(byte *)(iVar12 * 0xe + DAT_00902928 + -0xd);
            if (((bVar3 & 1) == 0) || ((bVar3 & 2) != 0)) {
              bVar1 = false;
            }
            else {
              bVar1 = true;
            }
            if (((bVar1) && (((&DAT_010262d0)[(uint)*(ushort *)(iVar13 + -8) * 4] & 1) != 0)) &&
               (((&DAT_010262d0)[(uint)*(ushort *)(iVar13 + -8) * 4] & 2) == 0)) {
              if ((bVar2 & 4) == 0) {
                bVar3 = *(byte *)((iVar12 + -4) * 0xe + DAT_00902928 + 1);
                if (((bVar3 & 1) == 0) || ((bVar3 & 2) != 0)) {
                  bVar1 = false;
                }
                else {
                  bVar1 = true;
                }
                if (((!bVar1) || (((&DAT_010262d0)[(uint)*(ushort *)(iVar13 + -0x32) * 4] & 1) == 0)
                    ) || (((&DAT_010262d0)[(uint)*(ushort *)(iVar13 + -0x32) * 4] & 2) != 0))
                goto LAB_006bae84;
              }
            }
            else {
LAB_006bae84:
              bVar3 = *(byte *)((iVar12 + -2) * 0xe + DAT_00902928 + 1);
              if (((bVar3 & 1) == 0) || ((bVar3 & 2) != 0)) {
                bVar1 = false;
              }
              else {
                bVar1 = true;
              }
              if (((!bVar1) || (((&DAT_010262d0)[(uint)*(ushort *)(iVar13 + -0x16) * 4] & 1) == 0))
                 || (((&DAT_010262d0)[(uint)*(ushort *)(iVar13 + -0x16) * 4] & 2) != 0)) {
                bVar3 = *(byte *)((iVar12 + -3) * 0xe + DAT_00902928 + 1);
                if (((bVar3 & 1) == 0) || ((bVar3 & 2) != 0)) {
                  bVar1 = false;
                }
                else {
                  bVar1 = true;
                }
                if (((!bVar1) || (((&DAT_010262d0)[(uint)*(ushort *)(iVar13 + -0x24) * 4] & 1) == 0)
                    ) || (((&DAT_010262d0)[(uint)*(ushort *)(iVar13 + -0x24) * 4] & 2) != 0)) {
                  iVar16 = (iVar17 - iVar16) * DAT_00902934 + iVar9;
                  bVar3 = *(byte *)((iVar16 + -3) * 0xe + DAT_00902928 + 1);
                  if (((bVar3 & 1) == 0) || ((bVar3 & 2) != 0)) {
                    bVar1 = false;
                  }
                  else {
                    bVar1 = true;
                  }
                  if (((!bVar1) ||
                      (((&DAT_010262d0)[(uint)*(ushort *)(iVar16 * 0xe + DAT_00902928 + -0x24) * 4]
                       & 1) == 0)) ||
                     (((&DAT_010262d0)[(uint)*(ushort *)(iVar16 * 0xe + DAT_00902928 + -0x24) * 4] &
                      2) != 0)) {
                    fVar20 = (float)VectorSignedToFloat(iVar9 << 4,(byte)(uVar18 >> 0x16) & 3);
                    if ((*(byte *)(iVar13 + 1) & 4) != 0) {
                      fVar20 = fVar20 + 8.0;
                    }
                    if ((bVar2 & 4) != 0) {
                      fVar20 = fVar20 - 8.0;
                    }
                    uVar18 = uVar11;
                    if ((fVar20 < fVar27) && (fVar27 - fVar20 <= DAT_006bb130)) {
                      fVar24 = *(float *)(param_1 + 0x13c);
                      *(float *)(param_1 + 0x13c) = fVar20 - fVar28;
                      *(int *)(param_1 + 0x14c) = (int)(fVar20 - fVar28);
                      *(float *)(param_1 + 0x130) =
                           ((fVar24 + fVar28) - fVar20) + *(float *)(param_1 + 0x130);
                      if (9.0 <= fVar27 - fVar20) {
                        *(undefined4 *)(param_1 + 0x134) = 0x40000000;
                      }
                      else {
                        *(undefined4 *)(param_1 + 0x134) = 0x3f800000;
                      }
                    }
                  }
                }
              }
            }
          }
          else {
LAB_006bade0:
            bVar2 = *(byte *)(iVar13 + -0xd);
            if ((bVar2 & 4) != 0) {
              bVar3 = *(byte *)(iVar12 * 0xe + DAT_00902928 + -0xd);
              if (((bVar3 & 1) == 0) || ((bVar3 & 2) != 0)) {
                bVar1 = false;
              }
              else {
                bVar1 = true;
              }
              if (bVar1) goto LAB_006bae10;
            }
          }
        }
      }
    }
    if (fVar21 != 0.0) {
      return;
    }
    uVar18 = uVar18 & 0xfffffff |
             (uint)(*(float *)(param_1 + 0x138) == *(float *)(param_1 + 0x180)) << 0x1e;
    if ((byte)(uVar18 >> 0x1e) != 0) {
      *(char *)(param_1 + 0x171) = -cVar5;
    }
    *(undefined4 *)(param_1 + 0x180) = 0xbf800000;
    iVar9 = FUN_0041e400(*(int *)(param_1 + 0x148) +
                         *(char *)(param_1 + 0x171) * 0xf + (*(int *)(param_1 + 0x150) >> 1) >> 4,1,
                         DAT_00902ea0 + -1);
    iVar12 = FUN_0041e400((int)(*(int *)(param_1 + 0x14c) + (uint)*(ushort *)(param_1 + 0x16a) +
                               -0x10) >> 4,2,DAT_00902ea4 + -1);
    uVar19 = DAT_00fd67fc;
    uVar11 = DAT_00fd67f8;
    iVar17 = DAT_00902934;
    iVar16 = DAT_00902928;
    uVar10 = DAT_00fd6800;
    if (*(char *)(param_1 + 0x118) != '\0') {
      iVar13 = iVar9 * DAT_00902934 + iVar12;
      bVar2 = *(byte *)((iVar13 + -2) * 0xe + DAT_00902928 + 1);
      if (((bVar2 & 1) == 0) || ((bVar2 & 2) != 0)) {
        bVar1 = false;
      }
      else {
        bVar1 = true;
      }
      if ((bVar1) && (*(short *)(iVar13 * 0xe + DAT_00902928 + -0x16) == 10)) {
        uVar10 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
        DAT_00fd67f4 = DAT_00fd67f8;
        DAT_00fd67f8 = DAT_00fd67fc;
        uVar10 = uVar10 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
        DAT_00fd67fc = DAT_00fd6800;
        fVar21 = (float)VectorSignedToFloat(uVar10 & 0x7fffffff,(byte)(uVar18 >> 0x16) & 3);
        if (((int)(fVar21 * fVar25 * 10.0) == 0) || (bVar7)) {
          if (DAT_00fe1ec4 == 1) {
            DAT_00fd67f4 = uVar11;
            DAT_00fd67f8 = uVar19;
            DAT_00fd6800 = uVar10;
            return;
          }
          DAT_00fd6800 = uVar10;
          iVar15 = FUN_00776dc8(iVar9,iVar12 + -2,(int)*(char *)(param_1 + 0x171));
          if (iVar15 != 0) {
            *(undefined1 *)(param_1 + 0x11b) = 1;
            *(short *)(param_1 + 0x1fa) = (short)iVar9;
            *(short *)(param_1 + 0x1fc) = (short)(iVar12 + -2);
            FUN_00648da4(iVar9,iVar12,iVar15);
            fVar25 = DAT_006bb12c;
            *(undefined1 *)(param_1 + 0x10c) = 1;
            *(float *)(param_1 + 0x17c) = fVar25 + *(float *)(param_1 + 0x17c);
            return;
          }
          *(char *)(param_1 + 0x171) = -*(char *)(param_1 + 0x171);
          *(undefined1 *)(param_1 + 0x10c) = 1;
          return;
        }
      }
    }
    DAT_00fd6800 = uVar10;
    fVar25 = *(float *)(param_1 + 0x140);
    uVar18 = uVar18 & 0xfffffff;
    if (((0.0 <= fVar25) || (*(char *)(param_1 + 0x1e1) != -1)) &&
       ((uVar18 = uVar18 | (uint)(fVar25 < 0.0) << 0x1f | (uint)(fVar25 == 0.0) << 0x1e,
        bVar2 = (byte)(uVar18 >> 0x18), (bool)(bVar2 >> 6 & 1) || (bool)(bVar2 >> 7) != NAN(fVar25)
        || (*(char *)(param_1 + 0x1e1) != '\x01')))) goto LAB_006bb49e;
    iVar13 = iVar9 * DAT_00902934 + iVar12;
    iVar14 = (iVar13 + -2) * 0xe + DAT_00902928;
    bVar2 = *(byte *)(iVar14 + 1);
    if (((bVar2 & 1) == 0) || ((bVar2 & 2) != 0)) {
      bVar7 = false;
    }
    else {
      bVar7 = true;
    }
    if ((bVar7) && (((&DAT_010262d0)[(uint)*(ushort *)(iVar14 + 6) * 4] & 3) == 1)) {
      bVar7 = true;
    }
    else {
      bVar7 = false;
    }
    if (bVar7) {
      if ((((*(char *)(param_1 + 0x171) < '\x01') ||
           (iVar16 = FUN_00609344(iVar9 + -2,iVar9 + -1,iVar12 + -5,iVar12 + -1), iVar16 != 0)) &&
          ((-1 < *(char *)(param_1 + 0x171) ||
           (iVar16 = FUN_00609344(iVar9 + 1,iVar9 + 2,iVar12 + -5,iVar12 + -1), iVar16 != 0)))) ||
         (iVar16 = FUN_00609344(iVar9,iVar9,iVar12 + -5,iVar12 + -3), iVar16 != 0)) {
        if (*(int *)(param_1 + 0x104) == 300) {
          iVar16 = FUN_007521fc((int)*(char *)(param_1 + 0x171) +
                                (*(int *)(param_1 + 0x148) + (*(int *)(param_1 + 0x150) >> 1) >> 4),
                                *(int *)(param_1 + 0x14c) + (*(int *)(param_1 + 0x154) >> 1) >> 4);
          if (iVar16 == 0) goto LAB_006bb242;
          *(undefined4 *)(param_1 + 0x140) = uVar22;
        }
LAB_006bb238:
        *(char *)(param_1 + 0x171) = -*(char *)(param_1 + 0x171);
      }
      else {
        *(undefined4 *)(param_1 + 0x144) = 0xc0c00000;
      }
LAB_006bb242:
      *(undefined1 *)(param_1 + 0x10c) = 1;
      iVar16 = DAT_00902928;
      iVar17 = DAT_00902934;
    }
    else {
      iVar14 = iVar13 * 0xe + DAT_00902928;
      bVar2 = *(byte *)(iVar14 + -0xd);
      if (((bVar2 & 1) == 0) || ((bVar2 & 2) != 0)) {
        bVar7 = false;
      }
      else {
        bVar7 = true;
      }
      if ((bVar7) && (((&DAT_010262d0)[(uint)*(ushort *)(iVar14 + -8) * 4] & 3) == 1)) {
        bVar7 = true;
      }
      else {
        bVar7 = false;
      }
      if (bVar7) {
        if ((((*(char *)(param_1 + 0x171) < '\x01') ||
             (iVar16 = FUN_00609344(iVar9 + -2,iVar9 + -1,iVar12 + -4,iVar12 + -1), iVar16 != 0)) &&
            ((-1 < *(char *)(param_1 + 0x171) ||
             (iVar16 = FUN_00609344(iVar9 + 1,iVar9 + 2,iVar12 + -4,iVar12 + -1), iVar16 != 0)))) ||
           (iVar16 = FUN_00609344(iVar9,iVar9,iVar12 + -4,iVar12 + -2), iVar16 != 0))
        goto LAB_006bb238;
        *(undefined4 *)(param_1 + 0x144) = 0xc0a00000;
        goto LAB_006bb242;
      }
      fVar21 = (float)VectorSignedToFloat(iVar12 << 4,(byte)(uVar18 >> 0x16) & 3);
      fVar25 = (float)VectorSignedToFloat((uint)*(ushort *)(param_1 + 0x16a),
                                          (byte)(uVar18 >> 0x16) & 3);
      if (20.0 < (fVar25 + *(float *)(param_1 + 0x13c)) - fVar21) {
        iVar14 = iVar13 * 0xe + DAT_00902928;
        iVar13 = FUN_00609074(iVar14);
        if ((iVar13 != 0) && ((*(byte *)(iVar14 + 1) & 0x18) == 0)) {
          if (((*(char *)(param_1 + 0x171) < '\x01') ||
              (iVar16 = FUN_00609344(iVar9 + -2,iVar9,iVar12 + -3,iVar12 + -1), iVar16 != 0)) &&
             ((-1 < *(char *)(param_1 + 0x171) ||
              (iVar16 = FUN_00609344(iVar9,iVar9 + 2,iVar12 + -3,iVar12 + -1), iVar16 != 0))))
          goto LAB_006bb238;
          *(undefined4 *)(param_1 + 0x144) = DAT_006bb4f0;
          goto LAB_006bb242;
        }
      }
    }
    if ((*(short *)(param_1 + 0x1f2) + -0x23 <= iVar15) &&
       (iVar15 <= *(short *)(param_1 + 0x1f2) + 0x23)) {
      iVar13 = iVar9 * iVar17 + iVar12;
      iVar15 = (iVar13 + 1) * 0xe + iVar16;
      bVar2 = *(byte *)(iVar15 + 1);
      if (((bVar2 & 1) == 0) || ((bVar2 & 2) != 0)) {
        bVar7 = false;
      }
      else {
        bVar7 = true;
      }
      if ((bVar7) && (((&DAT_010262d0)[(uint)*(ushort *)(iVar15 + 6) * 4] & 1) != 0)) {
        bVar7 = true;
      }
      else {
        bVar7 = false;
      }
      if (!bVar7) {
        cVar5 = *(char *)(param_1 + 0x171);
        iVar12 = (iVar9 - cVar5) * iVar17 + iVar12;
        iVar15 = (iVar12 + 1) * 0xe + iVar16;
        if (((*(byte *)(iVar15 + 1) & 1) == 0) ||
           (((&DAT_010262d0)[(uint)*(ushort *)(iVar15 + 6) * 4] & 1) == 0)) {
          bVar7 = false;
        }
        else {
          bVar7 = true;
        }
        if (((((((!bVar7) && (iVar15 = FUN_00655f64((iVar13 + 2) * 0xe + iVar16), iVar15 == 0)) &&
               (iVar15 = FUN_00655f98((iVar12 + 2) * 0xe + iVar16), iVar15 == 0)) &&
              ((iVar15 = FUN_00655f64((iVar13 + 3) * 0xe + iVar16), iVar15 == 0 &&
               (iVar15 = FUN_00655f98((iVar12 + 3) * 0xe + iVar16), iVar15 == 0)))) &&
             ((iVar15 = FUN_00655f64((iVar13 + 4) * 0xe + iVar16), iVar15 == 0 &&
              ((iVar15 = FUN_00655f64((iVar12 + 4) * 0xe + iVar16), iVar15 == 0 &&
               (iVar15 = *(int *)(param_1 + 0x104), iVar15 != 0x2e)))))) && (iVar15 != 0x94)) &&
           ((((iVar15 != 0x95 && (iVar15 != 0xe6)) && (iVar15 != 299)) &&
            ((iVar15 != 300 && (iVar15 != 0x12f)))))) {
          *(char *)(param_1 + 0x171) = -cVar5;
          *(undefined1 *)(param_1 + 0x10c) = 1;
          *(float *)(param_1 + 0x140) = -*(float *)(param_1 + 0x140);
        }
      }
    }
    if (0.0 <= *(float *)(param_1 + 0x144)) {
      return;
    }
    *(undefined4 *)(param_1 + 0x180) = *(undefined4 *)(param_1 + 0x138);
LAB_006bb49e:
    fVar25 = DAT_006bb4ec;
    if (*(float *)(param_1 + 0x144) < 0.0) {
      if (*(char *)(param_1 + 100) != '\0') {
        *(float *)(param_1 + 0x144) = *(float *)(param_1 + 0x144) * DAT_006bb4ec;
      }
      iVar15 = *(int *)(param_1 + 0x104);
      if (((iVar15 == 0x2e) || (iVar15 == 299)) || (iVar15 == 0x12f)) {
        *(float *)(param_1 + 0x144) = *(float *)(param_1 + 0x144) * fVar25;
      }
    }
    return;
  }
  fVar21 = *(float *)(param_1 + 0x180);
  uVar11 = uVar18 | (uint)(fVar21 < 0.0) << 0x1f | (uint)(fVar21 == 0.0) << 0x1e;
  uVar19 = uVar11 | (uint)NAN(fVar21) << 0x1c;
  bVar2 = (byte)(uVar11 >> 0x18);
  if (!(bool)(bVar2 >> 6 & 1) && bVar2 >> 7 == ((byte)(uVar19 >> 0x1c) & 1)) {
    *(float *)(param_1 + 0x180) = fVar21 - 1.0;
  }
  if ((((!bVar7) || (DAT_010703e0 != '\0')) || (bVar1)) ||
     (((iVar9 = *(int *)(param_1 + 0x104), iVar9 == 0x2e || (iVar9 == 0x94)) ||
      ((iVar9 == 0x95 || ((iVar9 == 0xe6 || (iVar9 == 299)))))))) {
LAB_006ba5de:
    if (*(int *)(param_1 + 0x104) == 300) goto LAB_006ba5e8;
  }
  else {
    if (iVar9 != 300) {
      if (iVar9 != 0x12f) {
        if (DAT_00fe1ec4 == 1) {
          return;
        }
        if ((iVar15 == *(short *)(param_1 + 0x1f2)) && (iVar16 == iVar17)) {
          fVar21 = *(float *)(param_1 + 0x140);
          if (fVar21 != 0.0) {
            *(undefined1 *)(param_1 + 0x10c) = 1;
          }
          uVar11 = uVar18 | (uint)(fVar21 < fVar20) << 0x1f | (uint)(fVar21 == fVar20) << 0x1e;
          uVar19 = uVar11 | (uint)(NAN(fVar21) || NAN(fVar20)) << 0x1c;
          bVar2 = (byte)(uVar11 >> 0x18);
          if ((bool)(bVar2 >> 6 & 1) || bVar2 >> 7 != ((byte)(uVar19 >> 0x1c) & 1)) {
            uVar19 = uVar18 | (uint)(DAT_006ba66c <= fVar21) << 0x1d;
            if ((byte)(uVar19 >> 0x1d) == 0) {
              *(float *)(param_1 + 0x140) = fVar21 + fVar20;
            }
            else {
              *(undefined4 *)(param_1 + 0x140) = uVar22;
            }
          }
          else {
            *(float *)(param_1 + 0x140) = fVar21 - fVar20;
          }
        }
        else {
          if (*(short *)(param_1 + 0x1f2) < iVar15) {
            *(undefined1 *)(param_1 + 0x171) = 0xff;
          }
          else {
            *(undefined1 *)(param_1 + 0x171) = 1;
          }
          *(undefined4 *)(param_1 + 0x178) = 0x3f800000;
          iVar9 = FUN_00608330(&DAT_00fd67f4,200);
          *(undefined4 *)(param_1 + 0x180) = uVar22;
          *(undefined1 *)(param_1 + 0x10c) = 1;
          uVar22 = VectorSignedToFloat(iVar9 + 200,(byte)(uVar19 >> 0x16) & 3);
          *(undefined4 *)(param_1 + 0x17c) = uVar22;
        }
        goto LAB_006ba758;
      }
      goto LAB_006ba5de;
    }
LAB_006ba5e8:
    *(float *)(param_1 + 0x140) = *(float *)(param_1 + 0x140) * 0.5;
  }
  fVar21 = *(float *)(param_1 + 0x140);
  uVar11 = uVar18 | (uint)(fVar21 < fVar20) << 0x1f | (uint)(fVar21 == fVar20) << 0x1e;
  uVar19 = uVar11 | (uint)(NAN(fVar21) || NAN(fVar20)) << 0x1c;
  bVar2 = (byte)(uVar11 >> 0x18);
  if ((bool)(bVar2 >> 6 & 1) || bVar2 >> 7 != ((byte)(uVar19 >> 0x1c) & 1)) {
    uVar19 = uVar18 | (uint)(DAT_006ba66c <= fVar21) << 0x1d;
    if ((byte)(uVar19 >> 0x1d) == 0) {
      *(float *)(param_1 + 0x140) = fVar21 + fVar20;
    }
    else {
      *(undefined4 *)(param_1 + 0x140) = uVar22;
    }
  }
  else {
    *(float *)(param_1 + 0x140) = fVar21 - fVar20;
  }
  if (DAT_00fe1ec4 == 1) {
    return;
  }
  if (0.0 < *(float *)(param_1 + 0x17c)) {
    *(float *)(param_1 + 0x17c) = *(float *)(param_1 + 0x17c) - 1.0;
  }
  uVar19 = uVar19 & 0xfffffff | (uint)(*(float *)(param_1 + 0x17c) == 0.0) << 0x1e |
           (uint)(0.0 <= *(float *)(param_1 + 0x17c)) << 0x1d;
  bVar2 = (byte)(uVar19 >> 0x18);
  if (!(bool)(bVar2 >> 5 & 1) || (bool)(bVar2 >> 6)) {
    *(undefined4 *)(param_1 + 0x178) = 0x3f800000;
    fVar21 = DAT_006ba668;
    uVar18 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar18 = uVar18 ^ (uVar18 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    DAT_00fd67fc = DAT_00fd6800;
    fVar20 = (float)VectorSignedToFloat(uVar18 & 0x7fffffff,(byte)(uVar19 >> 0x16) & 3);
    iVar9 = *(int *)(param_1 + 0x104);
    uVar23 = VectorSignedToFloat(200 - (int)(fVar20 * fVar25 * DAT_006ba668),
                                 (byte)(uVar19 >> 0x16) & 3);
    DAT_00fd6800 = uVar18;
    *(undefined4 *)(param_1 + 0x17c) = uVar23;
    if ((((((iVar9 == 0x2e) || (iVar9 == 0x94)) || (iVar9 == 0x95)) ||
         ((iVar9 == 0xe6 || (iVar9 == 299)))) || (iVar9 == 300)) || (iVar9 == 0x12f)) {
      uVar18 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
      DAT_00fd67f4 = DAT_00fd67f8;
      DAT_00fd67f8 = DAT_00fd67fc;
      uVar18 = uVar18 ^ (uVar18 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
      DAT_00fd67fc = DAT_00fd6800;
      fVar20 = (float)VectorSignedToFloat(uVar18 & 0x7fffffff,(byte)(uVar19 >> 0x16) & 3);
      fVar21 = (float)VectorSignedToFloat(200 - (int)(fVar20 * fVar25 * fVar21),
                                          (byte)(uVar19 >> 0x16) & 3);
      DAT_00fd6800 = uVar18;
      *(float *)(param_1 + 0x17c) = *(float *)(param_1 + 0x17c) + fVar21;
    }
    *(undefined4 *)(param_1 + 0x180) = uVar22;
    *(undefined1 *)(param_1 + 0x10c) = 1;
  }
LAB_006ba758:
  uVar11 = DAT_00fd67fc;
  uVar18 = DAT_00fd67f8;
  uVar22 = DAT_006ba9e8;
  if (DAT_00fe1ec4 == 1) {
    return;
  }
  if (bVar7) {
    if (iVar15 != *(short *)(param_1 + 0x1f2)) {
      return;
    }
    if (iVar16 != iVar17) {
      return;
    }
  }
  iVar16 = (int)*(short *)(param_1 + 0x1f2);
  if ((iVar16 + -0x19 <= iVar15) && (iVar15 <= iVar16 + 0x19)) {
    uVar10 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar10 = uVar10 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    DAT_00fd67fc = DAT_00fd6800;
    fVar21 = (float)VectorSignedToFloat(uVar10 & 0x7fffffff,(byte)(uVar19 >> 0x16) & 3);
    if ((int)(fVar21 * fVar25 * DAT_006ba9ec) != 0) {
      DAT_00fd67f4 = uVar18;
      DAT_00fd67f8 = uVar11;
      DAT_00fd6800 = uVar10;
      return;
    }
    if (*(float *)(param_1 + 0x180) != 0.0) {
      DAT_00fd67f4 = uVar18;
      DAT_00fd67f8 = uVar11;
      DAT_00fd6800 = uVar10;
      return;
    }
    DAT_00fd6800 = uVar10;
    *(undefined1 *)(param_1 + 0x10c) = 1;
    *(char *)(param_1 + 0x171) = -*(char *)(param_1 + 0x171);
    *(undefined4 *)(param_1 + 0x180) = uVar22;
    return;
  }
  if (*(float *)(param_1 + 0x180) != 0.0) {
    return;
  }
  if ((iVar15 < iVar16 + -0x32) && (*(char *)(param_1 + 0x171) < '\0')) {
    *(undefined1 *)(param_1 + 0x171) = 1;
    *(undefined1 *)(param_1 + 0x10c) = 1;
    return;
  }
  if (iVar15 <= iVar16 + 0x32) {
    return;
  }
  if (*(char *)(param_1 + 0x171) < '\x01') {
    return;
  }
  *(undefined1 *)(param_1 + 0x171) = 0xff;
  *(undefined1 *)(param_1 + 0x10c) = 1;
  return;
}


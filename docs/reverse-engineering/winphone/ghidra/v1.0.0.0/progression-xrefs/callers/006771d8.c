
void FUN_006771d8(void)

{
  byte bVar1;
  bool bVar2;
  bool bVar3;
  bool bVar4;
  bool bVar5;
  uint uVar6;
  undefined4 uVar7;
  undefined1 *puVar8;
  undefined1 *puVar9;
  int iVar10;
  int extraout_r2;
  int iVar11;
  uint uVar12;
  uint uVar13;
  uint uVar14;
  int iVar15;
  int iVar16;
  uint uVar17;
  uint in_fpscr;
  float fVar18;
  float fVar19;
  double dVar20;
  double dVar21;
  undefined1 auStack_fc [48];
  undefined1 auStack_cc [48];
  int local_9c;
  int local_98;
  int local_94;
  int local_90;
  int local_8c;
  int local_88;
  undefined1 *local_84;
  int local_80;
  undefined4 local_7c;
  int local_78;
  int local_74;
  int local_70;
  int local_6c;
  undefined8 local_68;
  undefined4 local_5c;
  undefined4 uStack_58;
  undefined1 auStack_54 [24];
  undefined1 auStack_3c [24];
  
  local_68 = FUN_007ffb80();
  local_84 = (undefined1 *)((ulonglong)local_68 >> 0x20);
  puVar8 = auStack_fc;
  local_5c = 0xfffffffe;
  uStack_58 = 0xfffffffe;
  local_7c = 0;
  local_70 = 0;
  local_9c = 0;
  local_6c = 0;
  local_90 = 0;
  local_80 = 0;
  local_94 = 0;
  local_88 = 0;
  local_74 = 0;
  local_78 = 0;
  local_98 = 0;
  iVar16 = 0;
  bVar5 = false;
  bVar3 = false;
  bVar2 = false;
  bVar4 = false;
  iVar10 = 0xc4;
  iVar11 = DAT_00fe1fa8;
  do {
    if (*(char *)(iVar11 + 0x100) != '\0') {
      iVar15 = *(int *)(iVar11 + 0x104);
      if (iVar15 == 0x11) {
        local_70 = 1;
      }
      else if (iVar15 == 0x12) {
        bVar5 = true;
      }
      else if (iVar15 == 0x13) {
        bVar3 = true;
      }
      else if (iVar15 == 0x14) {
        local_9c = 1;
      }
      else if (iVar15 == 0x25) {
        local_6c = 1;
      }
      else if (iVar15 == 0x26) {
        iVar16 = 1;
      }
      else if (iVar15 == 0x7c) {
        bVar2 = true;
      }
      else if (iVar15 == 0x6b) {
        local_90 = 1;
      }
      else if (iVar15 == 0x36) {
        local_80 = 1;
      }
      else if (iVar15 == 0xa0) {
        local_94 = 1;
      }
      else if (iVar15 == 0xb2) {
        local_88 = 1;
      }
      else if (iVar15 == 0xe5) {
        bVar4 = true;
      }
      else if (iVar15 == 0xd1) {
        local_74 = 1;
      }
      else if (iVar15 == 0xd0) {
        local_78 = 1;
      }
      else if (iVar15 == 0x16) {
        local_98 = 1;
      }
    }
    iVar11 = iVar11 + 0x274;
    iVar10 = iVar10 + -1;
  } while (iVar10 != 0);
  local_8c = iVar16;
  FUN_007ab078(auStack_54,&DAT_0080ed7b);
  puVar9 = local_84;
  uVar13 = DAT_00fd67fc;
  uVar14 = DAT_00fd67f8;
  uVar12 = DAT_00fd67f4;
  iVar11 = *(int *)((int)local_68 + 0x104);
  if (iVar11 == 0x11) {
    uVar6 = DAT_00fd6800;
    uVar17 = DAT_00fd67fc;
    if (DAT_00fe1f73 != '\0') {
LAB_006773f4:
      DAT_00fd67fc = uVar6;
      if (DAT_010703e0 == '\0') {
        if (DAT_010703e2 == '\0') {
          uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
          uVar12 = uVar12 ^ (uVar12 ^ uVar6 >> 0xb) >> 8;
          if (DAT_006779f0 <= DAT_010703dc) {
            uVar14 = in_fpscr & 0xfffffff | (uint)(DAT_010703dc < DAT_006779ec) << 0x1f |
                     (uint)(DAT_010703dc == DAT_006779ec) << 0x1e;
            bVar1 = (byte)(uVar14 >> 0x18);
            if ((bool)(bVar1 >> 6 & 1) ||
                (bool)(bVar1 >> 7) != (NAN(DAT_010703dc) || NAN(DAT_006779ec))) {
              DAT_00fd6800 = uVar12 ^ uVar6;
              fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,
                                                  (byte)(uVar14 >> 0x16) & 3);
              iVar11 = (int)(fVar18 * DAT_0067750c * 3.0);
              if (iVar11 == 0) {
                DAT_00fd67f4 = DAT_00fd67f8;
                DAT_00fd67f8 = uVar17;
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x16);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else if (iVar11 == 1) {
                DAT_00fd67f4 = DAT_00fd67f8;
                DAT_00fd67f8 = uVar17;
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x17);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else {
                DAT_00fd67f4 = DAT_00fd67f8;
                DAT_00fd67f8 = uVar17;
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x18);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
            }
            else {
              DAT_00fd6800 = uVar12 ^ uVar6;
              fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,
                                                  (byte)(uVar14 >> 0x16) & 3);
              if ((int)(fVar18 * DAT_0067750c * 2.0) == 0) {
                DAT_00fd67f4 = DAT_00fd67f8;
                DAT_00fd67f8 = uVar17;
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x14);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else {
                DAT_00fd67f4 = DAT_00fd67f8;
                DAT_00fd67f8 = uVar17;
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x15);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
            }
          }
          else {
            DAT_00fd6800 = uVar12 ^ uVar6;
            fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,
                                                (byte)((in_fpscr & 0xfffffff) >> 0x16) & 3);
            if ((int)(fVar18 * DAT_0067750c * 2.0) == 0) {
              DAT_00fd67f4 = DAT_00fd67f8;
              DAT_00fd67f8 = uVar17;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x12);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else {
              DAT_00fd67f4 = DAT_00fd67f8;
              DAT_00fd67f8 = uVar17;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x13);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
          }
        }
        else {
          uVar12 = uVar17;
          uVar14 = DAT_00fd67f8;
          if ((bVar5) && (bVar2)) {
            uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
            DAT_00fd67fc = uVar12 ^ (uVar12 ^ uVar6 >> 0xb) >> 8 ^ uVar6;
            fVar18 = (float)VectorSignedToFloat(DAT_00fd67fc & 0x7fffffff,
                                                (byte)(in_fpscr >> 0x16) & 3);
            DAT_00fd67f4 = DAT_00fd67f8;
            uVar12 = uVar6;
            uVar14 = uVar17;
            if ((int)(fVar18 * DAT_0067750c * 3.0) == 0) {
              DAT_00fd67f8 = uVar17;
              DAT_00fd6800 = DAT_00fd67fc;
              DAT_00fd67fc = uVar6;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xd);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
              goto LAB_00679f24;
            }
          }
          uVar13 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
          DAT_00fd6800 = uVar13 ^ (uVar13 ^ DAT_00fd67fc >> 0xb) >> 8 ^ DAT_00fd67fc;
          fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3
                                             );
          iVar11 = (int)(fVar18 * DAT_0067750c * 4.0);
          DAT_00fd67f4 = uVar14;
          DAT_00fd67f8 = uVar12;
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xe);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 2) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x10);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x11);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
      }
      else {
        uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
        uVar12 = uVar12 ^ (uVar12 ^ uVar6 >> 0xb) >> 8;
        if (DAT_00677508 <= DAT_010703dc) {
          uVar14 = in_fpscr & 0xfffffff | (uint)(DAT_010703dc < DAT_00677504) << 0x1f |
                   (uint)(DAT_010703dc == DAT_00677504) << 0x1e;
          bVar1 = (byte)(uVar14 >> 0x18);
          if ((bool)(bVar1 >> 6 & 1) ||
              (bool)(bVar1 >> 7) != (NAN(DAT_010703dc) || NAN(DAT_00677504))) {
            DAT_00fd6800 = uVar12 ^ uVar6;
            fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar14 >> 0x16) & 3
                                               );
            iVar11 = (int)(fVar18 * DAT_0067750c * 3.0);
            if (iVar11 == 0) {
              DAT_00fd67f4 = DAT_00fd67f8;
              DAT_00fd67f8 = uVar17;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,10);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 1) {
              DAT_00fd67f4 = DAT_00fd67f8;
              DAT_00fd67f8 = uVar17;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xb);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else {
              DAT_00fd67f4 = DAT_00fd67f8;
              DAT_00fd67f8 = uVar17;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xc);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
          }
          else {
            DAT_00fd6800 = uVar12 ^ uVar6;
            fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(uVar14 >> 0x16) & 3
                                               );
            iVar11 = (int)(fVar18 * DAT_0067750c * 3.0);
            if (iVar11 == 0) {
              DAT_00fd67f4 = DAT_00fd67f8;
              DAT_00fd67f8 = uVar17;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,7);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 1) {
              DAT_00fd67f4 = DAT_00fd67f8;
              DAT_00fd67f8 = uVar17;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,8);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else {
              DAT_00fd67f4 = DAT_00fd67f8;
              DAT_00fd67f8 = uVar17;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,9);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
          }
        }
        else {
          DAT_00fd6800 = uVar12 ^ uVar6;
          fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,
                                              (byte)((in_fpscr & 0xfffffff) >> 0x16) & 3);
          iVar11 = (int)(fVar18 * DAT_0067750c * 3.0);
          if (iVar11 == 0) {
            DAT_00fd67f4 = DAT_00fd67f8;
            DAT_00fd67f8 = uVar17;
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,4);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            DAT_00fd67f4 = DAT_00fd67f8;
            DAT_00fd67f8 = uVar17;
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,5);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            DAT_00fd67f4 = DAT_00fd67f8;
            DAT_00fd67f8 = uVar17;
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,6);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
      }
      goto LAB_00679f24;
    }
    DAT_00fd67f4 = DAT_00fd67f8;
    uVar12 = uVar12 ^ uVar12 << 0xb;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar6 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    DAT_00fd67fc = DAT_00fd6800;
    fVar18 = (float)VectorSignedToFloat(uVar6 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    uVar17 = DAT_00fd6800;
    if ((int)(fVar18 * DAT_0067750c * 3.0) != 0) goto LAB_006773f4;
    DAT_00fd6800 = uVar6;
    if (199 < *(short *)(extraout_r2 + 0x5d00)) {
      if (*(short *)(extraout_r2 + 0x5cfa) < 0xb) {
        uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,2);
        FUN_007aa80c(auStack_54,auStack_fc,uVar7);
        FUN_007aa754(auStack_fc);
      }
      else {
        uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,3);
        FUN_007aa80c(auStack_54,auStack_fc,uVar7);
        FUN_007aa754(auStack_fc);
      }
      goto LAB_00679f24;
    }
    uVar7 = FUN_0063a50c(auStack_fc,extraout_r2,1);
    FUN_007aa80c(auStack_54,auStack_cc,uVar7);
    FUN_007aa754(auStack_cc);
LAB_00679f28:
    FUN_007aa754(puVar8);
  }
  else {
    if (iVar11 == 0x12) {
      if (DAT_010703e2 == '\0') {
        DAT_00fd67f4 = DAT_00fd67f8;
        uVar12 = uVar12 ^ uVar12 << 0xb;
        DAT_00fd67f8 = DAT_00fd67fc;
        uVar12 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
        DAT_00fd67fc = DAT_00fd6800;
        fVar18 = (float)VectorSignedToFloat(uVar12 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
        if (((int)(fVar18 * DAT_00677d7c * 3.0) == 0) && (DAT_00fe1f7b == '\0')) {
          DAT_00fd6800 = uVar12;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x20);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          DAT_00fd67fc = uVar12;
          uVar6 = DAT_00fd6800;
          if (local_8c != 0) {
            uVar14 = uVar14 ^ uVar14 << 0xb;
            DAT_00fd67f4 = uVar13;
            DAT_00fd67fc = uVar14 ^ (uVar14 ^ uVar12 >> 0xb) >> 8 ^ uVar12;
            DAT_00fd67f8 = DAT_00fd6800;
            fVar18 = (float)VectorSignedToFloat(DAT_00fd67fc & 0x7fffffff,
                                                (byte)(in_fpscr >> 0x16) & 3);
            uVar14 = uVar13;
            uVar6 = uVar12;
            uVar13 = DAT_00fd6800;
            if ((int)(fVar18 * DAT_00677d7c * 4.0) == 0) {
              DAT_00fd6800 = DAT_00fd67fc;
              DAT_00fd67fc = uVar12;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x21);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
              goto LAB_00679f24;
            }
          }
          DAT_00fd67f4 = uVar14;
          DAT_00fd67f8 = uVar13;
          DAT_00fd6800 = DAT_00fd67fc;
          if (bVar3) {
            uVar14 = uVar14 ^ uVar14 << 0xb;
            DAT_00fd6800 = uVar14 ^ (uVar14 ^ DAT_00fd67fc >> 0xb) >> 8 ^ DAT_00fd67fc;
            fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,
                                                (byte)(in_fpscr >> 0x16) & 3);
            DAT_00fd67f4 = uVar13;
            DAT_00fd67f8 = uVar6;
            uVar6 = DAT_00fd67fc;
            if ((int)(fVar18 * DAT_00677d7c * 4.0) == 0) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x22);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
              goto LAB_00679f24;
            }
          }
          DAT_00fd67fc = uVar6;
          uVar12 = DAT_00fd67f8;
          uVar14 = DAT_00fd67fc;
          uVar13 = DAT_00fd6800;
          if (local_98 != 0) {
            uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
            uVar13 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
            fVar18 = (float)VectorSignedToFloat(uVar13 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
            DAT_00fd67f4 = DAT_00fd67f8;
            uVar12 = DAT_00fd67fc;
            uVar14 = DAT_00fd6800;
            if ((int)(fVar18 * DAT_00677d7c * 4.0) == 0) {
              DAT_00fd67f8 = DAT_00fd67fc;
              DAT_00fd67fc = DAT_00fd6800;
              DAT_00fd6800 = uVar13;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x23);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
              goto LAB_00679f24;
            }
          }
          DAT_00fd6800 = uVar13;
          DAT_00fd67fc = uVar14;
          DAT_00fd67f8 = uVar12;
          fVar18 = (float)VectorSignedToFloat((int)*(short *)(extraout_r2 + 0x5d00),
                                              (byte)(in_fpscr >> 0x16) & 3);
          fVar19 = (float)VectorSignedToFloat((int)*(short *)(extraout_r2 + 0x5d02),
                                              (byte)(in_fpscr >> 0x16) & 3);
          in_fpscr = in_fpscr & 0xfffffff;
          if (fVar18 * DAT_00677d78 <= fVar19) {
            dVar21 = (double)VectorSignedToFloat((int)*(short *)(extraout_r2 + 0x5d00),
                                                 (byte)(in_fpscr >> 0x16) & 3);
            dVar20 = (double)VectorSignedToFloat((int)*(short *)(extraout_r2 + 0x5d02),
                                                 (byte)(in_fpscr >> 0x16) & 3);
            if (dVar21 * DAT_00677d70 <= dVar20) {
              iVar11 = FUN_00608330(&DAT_00fd67f4,4);
              if (iVar11 == 0) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x30);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else if (iVar11 == 1) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x31);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else if (iVar11 == 2) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x32);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x33);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
            }
            else {
              iVar11 = FUN_00608330(&DAT_00fd67f4,7);
              if (iVar11 == 0) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x29);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else if (iVar11 == 1) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x2a);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else if (iVar11 == 2) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x2b);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else if (iVar11 == 3) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x2c);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else if (iVar11 == 4) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x2d);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else if (iVar11 == 5) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x2e);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
              else {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x2f);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
              }
            }
          }
          else {
            uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
            uVar12 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
            fVar18 = (float)VectorSignedToFloat(uVar12 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
            iVar11 = (int)(fVar18 * DAT_00677d7c * 5.0);
            DAT_00fd67f4 = DAT_00fd67f8;
            DAT_00fd67f8 = DAT_00fd67fc;
            if (iVar11 == 0) {
              DAT_00fd67fc = DAT_00fd6800;
              DAT_00fd6800 = uVar12;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x24);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 1) {
              DAT_00fd67fc = DAT_00fd6800;
              DAT_00fd6800 = uVar12;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x25);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 2) {
              DAT_00fd67fc = DAT_00fd6800;
              DAT_00fd6800 = uVar12;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x26);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 3) {
              DAT_00fd67fc = DAT_00fd6800;
              DAT_00fd6800 = uVar12;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x27);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else {
              DAT_00fd67fc = DAT_00fd6800;
              DAT_00fd6800 = uVar12;
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x28);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
          }
        }
      }
      else {
        uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
        dVar21 = (double)VectorSignedToFloat((int)*(short *)(extraout_r2 + 0x5d02),
                                             (byte)(in_fpscr >> 0x16) & 3);
        dVar20 = (double)VectorSignedToFloat((int)*(short *)(extraout_r2 + 0x5d00),
                                             (byte)(in_fpscr >> 0x16) & 3);
        DAT_00fd67f4 = DAT_00fd67f8;
        DAT_00fd67f8 = DAT_00fd67fc;
        uVar12 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8;
        DAT_00fd67fc = DAT_00fd6800;
        if (dVar20 * DAT_006779e4 <= dVar21) {
          DAT_00fd6800 = uVar12 ^ DAT_00fd6800;
          fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,
                                              (byte)((in_fpscr & 0xfffffff) >> 0x16) & 3);
          iVar11 = (int)(fVar18 * DAT_006779e0 * 4.0);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x1c);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x1d);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 2) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x1e);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x1f);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
        else {
          DAT_00fd6800 = uVar12 ^ DAT_00fd6800;
          fVar18 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,
                                              (byte)((in_fpscr & 0xfffffff) >> 0x16) & 3);
          iVar11 = (int)(fVar18 * DAT_006779e0 * 3.0);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x19);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x1a);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x1b);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
      }
LAB_00679f24:
      puVar8 = auStack_cc;
      goto LAB_00679f28;
    }
    if (iVar11 == 0x13) {
      if ((DAT_00fe1f7b == '\0') || (DAT_010338d3 != '\0')) {
        if (bVar5) {
          DAT_00fd67f4 = DAT_00fd67f8;
          uVar12 = uVar12 ^ uVar12 << 0xb;
          DAT_00fd67f8 = DAT_00fd67fc;
          uVar12 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
          DAT_00fd67fc = DAT_00fd6800;
          fVar18 = (float)VectorSignedToFloat(uVar12 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
          DAT_00fd6800 = uVar12;
          if ((int)(fVar18 * DAT_00678228 * 5.0) == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x3b);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        uVar12 = DAT_00fd67f8;
        uVar14 = DAT_00fd67fc;
        uVar13 = DAT_00fd6800;
        if (bVar5) {
          uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
          uVar13 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
          fVar18 = (float)VectorSignedToFloat(uVar13 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
          DAT_00fd67f4 = DAT_00fd67f8;
          uVar12 = DAT_00fd67fc;
          uVar14 = DAT_00fd6800;
          if ((int)(fVar18 * DAT_00678228 * 5.0) == 0) {
            DAT_00fd67f8 = DAT_00fd67fc;
            DAT_00fd67fc = DAT_00fd6800;
            DAT_00fd6800 = uVar13;
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x3c);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        DAT_00fd6800 = uVar13;
        DAT_00fd67fc = uVar14;
        DAT_00fd67f8 = uVar12;
        uVar12 = DAT_00fd67f8;
        uVar14 = DAT_00fd67fc;
        uVar13 = DAT_00fd6800;
        if (local_9c != 0) {
          uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
          uVar13 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
          fVar18 = (float)VectorSignedToFloat(uVar13 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
          DAT_00fd67f4 = DAT_00fd67f8;
          uVar12 = DAT_00fd67fc;
          uVar14 = DAT_00fd6800;
          if ((int)(fVar18 * DAT_00678228 * 5.0) == 0) {
            DAT_00fd67f8 = DAT_00fd67fc;
            DAT_00fd67fc = DAT_00fd6800;
            DAT_00fd6800 = uVar13;
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x3d);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        DAT_00fd6800 = uVar13;
        DAT_00fd67fc = uVar14;
        DAT_00fd67f8 = uVar12;
        uVar12 = DAT_00fd67f8;
        uVar14 = DAT_00fd67fc;
        uVar13 = DAT_00fd6800;
        if (local_8c != 0) {
          uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
          uVar13 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
          fVar18 = (float)VectorSignedToFloat(uVar13 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
          DAT_00fd67f4 = DAT_00fd67f8;
          DAT_00fd67f8 = DAT_00fd67fc;
          if ((int)(fVar18 * DAT_00678228 * 5.0) == 0) {
            DAT_00fd67fc = DAT_00fd6800;
            DAT_00fd6800 = uVar13;
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x3e);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
          uVar12 = DAT_00fd67fc;
          uVar14 = DAT_00fd6800;
          if (local_8c != 0) {
            DAT_00fd67fc = DAT_00fd6800;
            DAT_00fd6800 = uVar13;
            iVar11 = FUN_00608330(&DAT_00fd67f4,5);
            uVar12 = DAT_00fd67f8;
            uVar14 = DAT_00fd67fc;
            uVar13 = DAT_00fd6800;
            if (iVar11 == 0) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x3f);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
              goto LAB_00679f24;
            }
          }
        }
        DAT_00fd6800 = uVar13;
        DAT_00fd67fc = uVar14;
        DAT_00fd67f8 = uVar12;
        if (DAT_010703e2 == '\0') {
          iVar11 = FUN_00608330(&DAT_00fd67f4,3);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x42);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x43);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x44);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
        else {
          iVar11 = FUN_00608330(&DAT_00fd67f4,2);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x40);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x41);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
      }
      else {
        uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x3a);
        FUN_007aa80c(auStack_54,auStack_fc,uVar7);
        FUN_007aa754(auStack_fc);
      }
      goto LAB_00679f24;
    }
    if (iVar11 == 0x14) {
      uVar12 = DAT_01070468 ^ DAT_01070468 << 0xb;
      DAT_01070468 = DAT_0107046c;
      DAT_0107046c = DAT_01070470;
      uVar12 = uVar12 ^ (uVar12 ^ DAT_01070474 >> 0xb) >> 8 ^ DAT_01070474;
      DAT_01070470 = DAT_01070474;
      fVar18 = (float)VectorSignedToFloat(uVar12 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      DAT_01070474 = uVar12;
      if ((DAT_010338d3 != '\0') && ((int)(fVar18 * DAT_00678228 * 2.0) == 1)) {
        FUN_00636b34(local_84);
        local_7c = 1;
        FUN_007aa754(auStack_54);
        goto LAB_00679f6e;
      }
      if (DAT_00fe1f7a == '\0') {
        uVar12 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
        DAT_00fd67f4 = DAT_00fd67f8;
        DAT_00fd67f8 = DAT_00fd67fc;
        uVar12 = uVar12 ^ (uVar12 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
        DAT_00fd67fc = DAT_00fd6800;
        fVar18 = (float)VectorSignedToFloat(uVar12 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
        DAT_00fd6800 = uVar12;
        if ((int)(fVar18 * DAT_00678228 * 3.0) == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x45);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if (bVar3) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x46);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if (local_70 != 0) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x47);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if (local_6c != 0) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x48);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if (local_94 != 0) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xee);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if (DAT_010703e2 == '\0') {
        iVar11 = FUN_00608330(&DAT_00fd67f4,5);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x4d);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x4e);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x4f);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 3) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x50);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x51);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else {
        iVar11 = FUN_00608330(&DAT_00fd67f4,4);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x49);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x4a);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x4b);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x4c);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      goto LAB_00679f24;
    }
    if (iVar11 == 0x25) {
      if (DAT_010703e0 == '\0') {
        if ((*(short *)(extraout_r2 + 0x5d00) < 300) || (*(short *)(extraout_r2 + 0x5cfa) < 10)) {
          iVar11 = FUN_00608330(&DAT_00fd67f4,4);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x55);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x56);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 2) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x57);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x58);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
        else {
          iVar11 = FUN_00608330(&DAT_00fd67f4,4);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x59);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x5a);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 2) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x5b);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x5c);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
      }
      else {
        iVar11 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x52);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x53);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x54);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      goto LAB_00679f24;
    }
    if (iVar11 == 0x26) {
      if (DAT_00fe1f7a == '\0') {
        iVar11 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x5d);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          FUN_007aa754(auStack_cc);
        }
      }
      if (DAT_010703e2 == '\0') {
        if (bVar3) {
          iVar11 = FUN_00608330(&DAT_00fd67f4,5);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x61);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            iVar11 = FUN_00608330(&DAT_00fd67f4,5);
            if (iVar11 != 0) goto LAB_006787f8;
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x62);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
        else {
LAB_006787f8:
          if (bVar5) {
            iVar11 = FUN_00608330(&DAT_00fd67f4,4);
            if (iVar11 == 0) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,99);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
              goto LAB_00679f24;
            }
          }
          if (local_9c != 0) {
            iVar11 = FUN_00608330(&DAT_00fd67f4,4);
            if (iVar11 == 0) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,100);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
              goto LAB_00679f24;
            }
          }
          if (DAT_010703e0 == '\0') {
            iVar11 = FUN_00608330(&DAT_00fd67f4,4);
            if (iVar11 == 0) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x65);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 1) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x66);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 2) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x67);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x68);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
          }
          else {
            iVar11 = FUN_00608330(&DAT_00fd67f4,5);
            if (iVar11 == 0) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x69);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 1) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x6a);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 2) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x6b);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 3) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x6c);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x6d);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
          }
        }
      }
      else {
        iVar11 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x5e);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x5f);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x60);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      goto LAB_00679f24;
    }
    if (iVar11 == 0x36) {
      if (!bVar2) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,2);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x6e);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if (local_94 != 0) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xed);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if (DAT_010703e2 == '\0') {
        if (bVar5) {
          iVar11 = FUN_00608330(&DAT_00fd67f4,4);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x70);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (*(short *)(extraout_r2 + 0x5c98) == 0x18) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x71);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          iVar11 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x72);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x73);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 2) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x74);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 3) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x75);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 4) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x76);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x77);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
      }
      else {
        uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x6f);
        FUN_007aa80c(auStack_54,auStack_fc,uVar7);
        FUN_007aa754(auStack_fc);
      }
      goto LAB_00679f24;
    }
    if (iVar11 == 0x69) {
      uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x78);
      FUN_007aa80c(auStack_54,auStack_fc,uVar7);
      FUN_007aa754(auStack_fc);
      goto LAB_00679f24;
    }
    if (iVar11 == 0x6b) {
      if (*(char *)((int)local_68 + 0x119) == '\0') {
        if (bVar2) {
          iVar11 = FUN_00608330(&DAT_00fd67f4,4);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x7e);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (DAT_010703e0 == '\0') {
          iVar11 = FUN_00608330(&DAT_00fd67f4,5);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x7f);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x80);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 2) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x81);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 3) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x82);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x83);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
        else {
          iVar11 = FUN_00608330(&DAT_00fd67f4,5);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x84);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x85);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 2) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x86);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 3) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x87);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x88);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
      }
      else {
        iVar11 = FUN_00608330(&DAT_00fd67f4,5);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x79);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x7a);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x7b);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 3) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x7c);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x7d);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      goto LAB_00679f24;
    }
    if (iVar11 == 0x6a) {
      uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x89);
      FUN_007aa80c(auStack_54,auStack_fc,uVar7);
      FUN_007aa754(auStack_fc);
      goto LAB_00679f24;
    }
    if (iVar11 != 0x6c) {
      if (iVar11 == 0x7b) {
        uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x9c);
        FUN_007aa80c(auStack_54,auStack_fc,uVar7);
        FUN_007aa754(auStack_fc);
      }
      else if (iVar11 == 0x7c) {
        if (*(char *)((int)local_68 + 0x119) == '\0') {
          if (DAT_010703e2 == '\0') {
            if (local_90 != 0) {
              iVar11 = FUN_00608330(&DAT_00fd67f4,6);
              if (iVar11 == 0) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa5);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
                goto LAB_00679f24;
              }
            }
            if (bVar3) {
              iVar11 = FUN_00608330(&DAT_00fd67f4,6);
              if (iVar11 == 0) {
                uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa6);
                FUN_007aa80c(auStack_54,auStack_fc,uVar7);
                FUN_007aa754(auStack_fc);
                goto LAB_00679f24;
              }
            }
            iVar11 = FUN_00608330(&DAT_00fd67f4,3);
            if (iVar11 == 0) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa7);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 1) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa8);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa9);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
          }
          else {
            iVar11 = FUN_00608330(&DAT_00fd67f4,4);
            if (iVar11 == 0) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa1);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 1) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa2);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 2) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa3);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa4);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
          }
        }
        else {
          iVar11 = FUN_00608330(&DAT_00fd67f4,4);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x9d);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x9e);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 2) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x9f);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xa0);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
      }
      else if (iVar11 == 0x16) {
        if (DAT_010703e2 == '\0') {
          if (DAT_010703e0 == '\0') {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xad);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            iVar11 = FUN_00608330(&DAT_00fd67f4,3);
            if (iVar11 == 0) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xae);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else if (iVar11 == 1) {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xaf);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
            else {
              uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xb0);
              FUN_007aa80c(auStack_54,auStack_fc,uVar7);
              FUN_007aa754(auStack_fc);
            }
          }
        }
        else {
          iVar11 = FUN_00608330(&DAT_00fd67f4,3);
          if (iVar11 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xaa);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else if (iVar11 == 1) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xab);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
          else {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xac);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
          }
        }
      }
      else if (iVar11 == 0x8e) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xe0);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xe1);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 2) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xe2);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else if (iVar11 == 0xa0) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (local_9c != 0) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xe8);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (local_80 != 0) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xec);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xe7);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xe9);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xea);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 3) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xeb);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 4) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf0);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 5) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf1);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else if (iVar11 == 0xb2) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,5);
        if (DAT_010703e2 != '\0') {
          iVar10 = FUN_00608330(&DAT_00fd67f4,3);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf5);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (local_74 != 0) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf6);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (bVar4) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf7);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf2);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf3);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf4);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 3) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf8);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 4) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xf9);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else if (iVar11 == 0xcf) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,3);
        if (bVar4) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x104);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x101);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x102);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 2) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x103);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else if (iVar11 == 0xd0) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,7);
        if (*(char *)(extraout_r2 + 0xdb) != '\0') {
          iVar10 = FUN_00608330(&DAT_00fd67f4,5);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x10c);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x109);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x10a);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x10b);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 3) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x10d);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 4) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x10e);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 5) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x10f);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 6) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x110);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else if (iVar11 == 0xd1) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,5);
        if (bVar4) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x11c);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (local_88 != 0) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x11b);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x118);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x119);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x11a);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 3) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x11d);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 4) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x11e);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else if (iVar11 == 0xe3) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,5);
        if (DAT_010338d3 != '\0') {
          iVar10 = FUN_00608330(&DAT_00fd67f4,7);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xfa);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (local_78 != 0) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xfb);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xfc);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xfd);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xfe);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 3) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0xff);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 4) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x100);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else if (iVar11 == 0xe4) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,3);
        if (bVar5) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x107);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x105);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x106);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 2) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x108);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else {
        if (iVar11 != 0xe5) goto LAB_00679f2c;
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (*(char *)(extraout_r2 + 0xdb) == '\0') {
          iVar10 = FUN_00608330(&DAT_00fd67f4,5);
          if (iVar10 == 0) {
            uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x114);
            FUN_007aa80c(auStack_54,auStack_fc,uVar7);
            FUN_007aa754(auStack_fc);
            goto LAB_00679f24;
          }
        }
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x111);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x112);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x113);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 3) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x115);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 4) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x116);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 5) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x117);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      goto LAB_00679f24;
    }
    if (*(char *)((int)local_68 + 0x119) == '\0') {
      if ((*(char *)(extraout_r2 + 0xdb) != '\0') && (local_98 != 0)) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x8e);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if ((*(char *)(extraout_r2 + 0xdb) != '\0') && (iVar16 != 0)) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x8f);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if ((*(char *)(extraout_r2 + 0xdb) != '\0') && (local_90 != 0)) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x90);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if ((*(char *)(extraout_r2 + 0xdb) == '\0') && (bVar5)) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x91);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if ((*(char *)(extraout_r2 + 0xdb) == '\0') && (bVar2)) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x92);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if ((*(char *)(extraout_r2 + 0xdb) == '\0') && (local_9c != 0)) {
        iVar11 = FUN_00608330(&DAT_00fd67f4,6);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x93);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
          goto LAB_00679f24;
        }
      }
      if (DAT_010703e0 == '\0') {
        iVar11 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x94);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x95);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          if (iVar11 != 2) goto LAB_00679f2c;
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x96);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      else {
        iVar11 = FUN_00608330(&DAT_00fd67f4,5);
        if (iVar11 == 0) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x97);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 1) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x98);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 2) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x99);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else if (iVar11 == 3) {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x9a);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
        else {
          uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x9b);
          FUN_007aa80c(auStack_54,auStack_fc,uVar7);
          FUN_007aa754(auStack_fc);
        }
      }
      goto LAB_00679f24;
    }
    iVar11 = FUN_00608330(&DAT_00fd67f4,3);
    if (iVar11 == 0) {
      uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x8a);
      FUN_007aa80c(auStack_54,auStack_fc,uVar7);
      FUN_007aa754(auStack_fc);
      goto LAB_00679f24;
    }
    if (iVar11 == 1) {
      if (*(char *)(extraout_r2 + 0xdb) == '\0') {
        uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x8b);
        FUN_007aa80c(auStack_54,auStack_fc,uVar7);
        FUN_007aa754(auStack_fc);
      }
      else {
        uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x8c);
        FUN_007aa80c(auStack_54,auStack_fc,uVar7);
        FUN_007aa754(auStack_fc);
      }
      goto LAB_00679f24;
    }
    if (iVar11 == 2) {
      uVar7 = FUN_0063a50c(auStack_cc,extraout_r2,0x8d);
      FUN_007aa80c(auStack_54,auStack_fc,uVar7);
      FUN_007aa754(auStack_fc);
      goto LAB_00679f24;
    }
  }
LAB_00679f2c:
  puVar9 = local_84;
  *(undefined4 *)(local_84 + 0x14) = 0xf;
  *(undefined4 *)(local_84 + 0x10) = 0;
  *local_84 = 0;
  FUN_0041bae0(local_84,auStack_54,0,0xffffffff);
  *(undefined4 *)(puVar9 + 0x2c) = 7;
  *(undefined4 *)(puVar9 + 0x28) = 0;
  *(undefined2 *)(puVar9 + 0x18) = 0;
  FUN_00425168(puVar9 + 0x18,auStack_3c,0,0xffffffff);
  local_7c = 1;
  FUN_007aa754(auStack_54);
LAB_00679f6e:
  FUN_007ffb98(puVar9);
  return;
}


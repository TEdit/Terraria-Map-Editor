
/* WARNING: Heritage AFTER dead removal. Example location: s1 : 0x006db1a6 */
/* WARNING: Restarted to delay deadcode elimination for space: register */

void FUN_006da85c(void)

{
  uint uVar1;
  ushort uVar2;
  short sVar3;
  int iVar4;
  undefined4 uVar5;
  undefined4 uVar6;
  int *piVar7;
  int *piVar8;
  undefined1 *puVar9;
  int extraout_r2;
  int iVar10;
  int iVar11;
  bool bVar12;
  int iVar13;
  int iVar14;
  int iVar15;
  int iVar16;
  uint in_fpscr;
  uint uVar17;
  undefined8 in_d0;
  double dVar18;
  float fVar19;
  float fVar20;
  undefined8 in_d1;
  double dVar21;
  float fVar22;
  undefined8 uVar23;
  undefined1 auStack_3b4 [24];
  undefined4 local_39c;
  undefined4 uStack_398;
  undefined1 auStack_394 [24];
  undefined1 auStack_37c [24];
  undefined1 auStack_364 [48];
  undefined1 auStack_334 [48];
  undefined1 auStack_304 [48];
  undefined1 auStack_2d4 [48];
  undefined1 auStack_2a4 [48];
  undefined1 auStack_274 [48];
  undefined1 auStack_244 [48];
  undefined1 auStack_214 [48];
  undefined1 auStack_1e4 [48];
  undefined1 auStack_1b4 [48];
  undefined1 auStack_184 [48];
  undefined1 auStack_154 [48];
  undefined1 auStack_124 [48];
  undefined1 auStack_f4 [48];
  undefined1 auStack_c4 [48];
  undefined1 auStack_94 [48];
  undefined1 auStack_64 [64];
  
  uVar23 = FUN_007ffb80();
  iVar10 = (int)((ulonglong)uVar23 >> 0x20);
  iVar4 = (int)uVar23;
  local_39c = 0xfffffffe;
  uStack_398 = 0xfffffffe;
  iVar15 = iVar10 >> 4;
  iVar14 = extraout_r2 >> 4;
  iVar13 = (DAT_00902934 * iVar15 + iVar14) * 0xe + DAT_00902928;
  if ((*(byte *)(iVar13 + 1) & 1) == 0) goto switchD_006daf52_caseD_81;
  uVar2 = *(ushort *)(iVar13 + 6);
  if (0x7d < uVar2) {
    if (0xcf < uVar2) {
      if (uVar2 < 0xd9) {
        if (uVar2 == 0xd8) {
          FUN_007523e8(iVar15,iVar14);
        }
        else if (uVar2 == 0xd1) {
          FUN_00754554(iVar15,iVar14);
        }
        else if ((uVar2 == 0xd4) && (*(char *)(iVar4 + 0xb7) == '\0')) {
          iVar16 = 0;
          iVar10 = iVar4;
          do {
            if ((*(int *)(iVar10 + 0x94c) == 0x3b5) && (0 < *(short *)(iVar10 + 0x98a))) {
              iVar10 = iVar4 + iVar16 * 0xa0;
              sVar3 = *(short *)(iVar10 + 0x98a) + -1;
              *(short *)(iVar10 + 0x98a) = sVar3;
              if (sVar3 < 1) {
                FUN_00612a6c(iVar10 + 0x948);
              }
              *(undefined1 *)(iVar4 + 0xb7) = 10;
              FUN_007a8384(2,*(undefined4 *)(iVar4 + 0x100),*(undefined4 *)(iVar4 + 0x104),0xb);
              iVar10 = (int)((ulonglong)
                             ((longlong)(int)*(short *)(iVar13 + 10) * (longlong)DAT_006db304) >>
                            0x20);
              iVar16 = (iVar10 >> 2) - (iVar10 >> 0x1f);
              iVar10 = (int)((ulonglong)
                             ((longlong)(int)*(short *)(iVar13 + 0xc) * (longlong)DAT_006db304) >>
                            0x20);
              iVar11 = (iVar10 >> 2) - (iVar10 >> 0x1f);
              uVar5 = FUN_00608330(&DAT_00fd67f4,0x1c2);
              fVar19 = (float)VectorSignedToFloat(uVar5,(byte)(in_fpscr >> 0x16) & 3);
              fVar19 = fVar19 * DAT_006db300;
              uVar5 = FUN_005c4e6c(&DAT_00fd67f4,0x55,0x69);
              fVar22 = (float)VectorSignedToFloat(uVar5,(byte)(in_fpscr >> 0x16) & 3);
              uVar5 = FUN_005c4e6c(&DAT_00fd67f4,0xffffffdd,0xb);
              iVar10 = (int)((ulonglong)((longlong)iVar16 * (longlong)DAT_006db2fc) >> 0x20);
              fVar20 = (float)VectorSignedToFloat(uVar5,(byte)(in_fpscr >> 0x16) & 3);
              iVar10 = iVar10 - (iVar10 >> 0x1f);
              iVar13 = (int)((ulonglong)((longlong)iVar11 * (longlong)DAT_006db2fc) >> 0x20);
              VectorSignedToFloat(((iVar10 * 3 - iVar16) + iVar15) * 0x10 + 0x18,
                                  (byte)(in_fpscr >> 0x16) & 3);
              VectorSignedToFloat((((iVar13 - (iVar13 >> 0x1f)) * 3 - iVar11) + iVar14) * 0x10 +
                                  0x18,(byte)(in_fpscr >> 0x16) & 3);
              if (iVar10 == 0) {
                fVar22 = -fVar22;
              }
              FUN_006f7004((int)in_d0,(int)((ulonglong)in_d0 >> 0x20),
                           ((fVar19 + 12.0) / SQRT(fVar22 * fVar22 + fVar20 * fVar20)) * fVar22,
                           (int)((ulonglong)in_d1 >> 0x20),0x40600000,0xa6,0x11,
                           *(undefined1 *)(iVar4 + 0x5cc9));
              break;
            }
            iVar16 = iVar16 + 1;
            iVar10 = iVar10 + 0xa0;
          } while (iVar16 < 0x30);
        }
      }
      else if (uVar2 == 0xdb) {
        *(short *)(iVar4 + 0x8960) = (short)((int)(iVar10 + ((uint)(iVar10 >> 3) >> 0x1c)) >> 4);
        *(short *)(iVar4 + 0x8962) =
             (short)((int)(extraout_r2 + ((uint)(extraout_r2 >> 3) >> 0x1c)) >> 4);
        *(undefined2 *)(iVar4 + 0x154) = 1;
        *(undefined1 *)(iVar4 + 0x5cba) = 1;
        FUN_006d74f0(iVar4);
      }
      else if (((uVar2 == 0xed) && (iVar15 = FUN_00656370(0xf5), iVar15 == 0)) &&
              (DAT_00fe1f8d != '\0')) {
        iVar14 = 0;
        iVar15 = iVar4;
        do {
          if (*(int *)(iVar15 + 0x94c) == 0x50d) {
            iVar15 = iVar4 + iVar14 * 0xa0;
            sVar3 = *(short *)(iVar15 + 0x98a) + -1;
            *(short *)(iVar15 + 0x98a) = sVar3;
            if (sVar3 < 1) {
              FUN_00612a6c(iVar15 + 0x948);
            }
            FUN_007a8384(0xf,*(undefined4 *)(iVar4 + 0x100),*(undefined4 *)(iVar4 + 0x104),0);
            if (DAT_00fe1ec4 == 1) {
              FUN_0064937c(*(undefined1 *)(iVar4 + 0x5cc9),0xf5);
            }
            else {
              FUN_00685904(iVar4,0xf5);
            }
            break;
          }
          iVar14 = iVar14 + 1;
          iVar15 = iVar15 + 0xa0;
        } while (iVar14 < 0x30);
      }
      goto switchD_006daf52_caseD_81;
    }
    if (uVar2 == 0xcf) {
      FUN_007a8384(0x1c,iVar10,extraout_r2,0);
      FUN_00754178(iVar15,iVar14);
      uVar5 = 1;
      goto LAB_006db286;
    }
    switch(uVar2) {
    case 0x80:
      iVar4 = (int)((ulonglong)((longlong)(int)*(short *)(iVar13 + 10) * (longlong)DAT_006db308) >>
                   0x20);
      iVar4 = (int)*(short *)(iVar13 + 10) + ((iVar4 >> 5) - (iVar4 >> 0x1f)) * -100;
      iVar10 = (int)((ulonglong)((longlong)iVar4 * (longlong)DAT_006db304) >> 0x20);
      iVar4 = iVar4 + ((iVar10 >> 3) - (iVar10 >> 0x1f)) * -0x24;
      if (iVar4 == 0x12) {
        iVar15 = iVar15 + -1;
        iVar4 = (int)*(short *)((DAT_00902934 * iVar15 + iVar14) * 0xe + DAT_00902928 + 10);
      }
      if (99 < iVar4) {
        FUN_007743f8(iVar15,iVar14,1,0,0,0);
        FUN_00647acc(0,iVar15,iVar14,1,0);
        uVar5 = 1;
        break;
      }
    default:
switchD_006daf52_caseD_81:
      uVar5 = 0;
      break;
    case 0x84:
    case 0x88:
    case 0x90:
      FUN_00772d30(iVar15,iVar14);
      FUN_00649310(iVar15,iVar14);
      uVar5 = 1;
      break;
    case 0x8b:
      FUN_007a8384(0x1c,iVar10,extraout_r2,0);
      FUN_0075425c(iVar15,iVar14);
      uVar5 = 1;
    }
LAB_006db286:
    FUN_007ffb98(uVar5);
    return;
  }
  if (uVar2 == 0x7d) {
    FUN_006c48cc(iVar4,0x1d,36000,1);
    FUN_007a8384(2,*(undefined4 *)(iVar4 + 0x100),*(undefined4 *)(iVar4 + 0x104),4);
    uVar5 = 1;
    goto LAB_006db286;
  }
  if (uVar2 < 0x22) {
    if (uVar2 == 0x21) {
LAB_006da9ea:
      FUN_00761b80(iVar15,iVar14);
      FUN_00647acc(0,iVar15,iVar14,0,0);
      uVar5 = 1;
      goto LAB_006db286;
    }
    if (uVar2 < 0xe) {
      if ((uVar2 == 0xd) || (uVar2 == 4)) goto LAB_006da9ea;
      if (uVar2 == 10) {
        if ((*(short *)(iVar13 + 0xc) < 0x252) || (0x286 < *(short *)(iVar13 + 0xc))) {
          iVar4 = FUN_00776dc8(iVar15,iVar14,(int)*(char *)(iVar4 + 0x5cc8));
          if (iVar4 != 0) {
            iVar10 = FUN_0058ef48();
            *(int *)(iVar10 + 0x3488) = *(int *)(iVar10 + 0x3488) + 1;
            FUN_00648da4(iVar15,iVar14,iVar4);
            uVar5 = 1;
            goto LAB_006db286;
          }
        }
        else {
          iVar13 = 0;
          iVar10 = iVar4;
          do {
            if ((*(int *)(iVar10 + 0x94c) == 0x475) && (0 < *(short *)(iVar10 + 0x98a))) {
              iVar10 = FUN_00755fb4(iVar15,iVar14);
              if (iVar10 != 0) {
                iVar10 = iVar4 + iVar13 * 0xa0;
                sVar3 = *(short *)(iVar10 + 0x98a) + -1;
                *(short *)(iVar10 + 0x98a) = sVar3;
                if (sVar3 < 1) {
                  FUN_00612a6c(iVar10 + 0x948);
                }
                FUN_00648744(*(undefined1 *)(iVar4 + 0x5cc9),iVar15,iVar14);
              }
              break;
            }
            iVar13 = iVar13 + 1;
            iVar10 = iVar10 + 0xa0;
          } while (iVar13 < 0x30);
        }
      }
      else if ((uVar2 == 0xb) && (iVar4 = FUN_00772624(iVar15,iVar14,0), iVar4 != 0)) {
        iVar4 = FUN_0058ef48();
        *(int *)(iVar4 + 0x348c) = *(int *)(iVar4 + 0x348c) + 1;
        FUN_006494f4(iVar15,iVar14);
        uVar5 = 1;
        goto LAB_006db286;
      }
    }
    else if ((uVar2 == 0x15) || (uVar2 == 0x1d)) goto LAB_006dac90;
  }
  else if (uVar2 < 0x50) {
    if (uVar2 == 0x4f) {
      iVar10 = (int)((ulonglong)((longlong)(int)*(short *)(iVar13 + 10) * (longlong)DAT_006dac8c) >>
                    0x20);
      iVar15 = iVar15 - ((iVar10 >> 2) - (iVar10 >> 0x1f));
      if (*(short *)(iVar13 + 10) < 0x48) {
        iVar15 = iVar15 + 2;
      }
      else {
        iVar15 = iVar15 + 5;
      }
      iVar10 = (int)((ulonglong)((longlong)(int)*(short *)(iVar13 + 0xc) * (longlong)DAT_006dac8c)
                    >> 0x20);
      iVar14 = iVar14 - ((iVar10 >> 2) - (iVar10 >> 0x1f) & 1U);
      iVar10 = FUN_006c3490(iVar15,iVar14 + 2);
      if (iVar10 != 0) {
        FUN_006c0828(iVar4,iVar15,iVar14 + 2);
        FUN_0041c4d8(auStack_3b4,"MENU[57]");
        uVar5 = FUN_005f82b0(auStack_2d4,auStack_3b4);
        FUN_006050e4(uVar5,0xff,0xf0,0x14,600);
        FUN_007aa754(auStack_2d4);
        FUN_0041b8a0(auStack_3b4);
        uVar5 = 1;
        goto LAB_006db286;
      }
    }
    else if ((uVar2 == 0x31) || ((uVar2 == 0x32 && (*(short *)(iVar13 + 10) == 0x5a))))
    goto LAB_006da9ea;
  }
  else if (uVar2 == 0x61) {
LAB_006dac90:
    if (*(short *)(iVar4 + 0x8df0) == -1) {
      iVar10 = (int)*(short *)(iVar13 + 10);
      iVar16 = -1;
      iVar11 = (int)((ulonglong)((longlong)iVar10 * (longlong)DAT_006db304) >> 0x20);
      iVar15 = iVar15 - ((iVar11 >> 2) - (iVar11 >> 0x1f) & 1U);
      iVar11 = (int)((ulonglong)((longlong)(int)*(short *)(iVar13 + 0xc) * (longlong)DAT_006db304)
                    >> 0x20);
      iVar14 = iVar14 - ((iVar11 >> 2) - (iVar11 >> 0x1f));
      if (uVar2 == 0x1d) {
        iVar16 = -2;
      }
      else if (uVar2 == 0x61) {
        iVar16 = -3;
      }
      else {
        if (iVar10 < 0xfc) {
          if (iVar10 < 0xd8) {
            if (iVar10 < 0xb4) {
              uVar5 = FUN_00633a94(auStack_154,0x30);
              iVar10 = FUN_0058ef48();
              FUN_007aa80c(iVar10 + 0x33b4,auStack_124,uVar5);
              FUN_007aa754(auStack_124);
              puVar9 = auStack_154;
            }
            else {
              uVar5 = FUN_00633a94(auStack_184,0x157);
              iVar10 = FUN_0058ef48();
              FUN_007aa80c(iVar10 + 0x33b4,auStack_1e4,uVar5);
              FUN_007aa754(auStack_1e4);
              puVar9 = auStack_184;
            }
          }
          else {
            uVar5 = FUN_00633a94(auStack_244,0x15c);
            iVar10 = FUN_0058ef48();
            FUN_007aa80c(iVar10 + 0x33b4,auStack_2a4,uVar5);
            FUN_007aa754(auStack_2a4);
            puVar9 = auStack_244;
          }
        }
        else {
          uVar5 = FUN_00633a94(auStack_304,0x30);
          iVar10 = FUN_0058ef48();
          FUN_007aa80c(iVar10 + 0x33b4,auStack_334,uVar5);
          FUN_007aa754(auStack_334);
          puVar9 = auStack_304;
        }
        FUN_007aa754(puVar9);
      }
      iVar10 = (int)*(short *)(iVar13 + 10);
      if (DAT_00fe1ec4 == 1) {
        if (iVar16 != -1) goto LAB_006daee8;
        if ((((0x47 < iVar10) && (iVar10 < 0x6b)) || ((0x8f < iVar10 && (iVar10 < 0xb3)))) ||
           ((0x33b < iVar10 && (iVar10 < 0x3ef)))) goto LAB_006dae2e;
        if (((iVar15 == *(short *)(iVar4 + 0x8de4)) && (iVar14 == *(short *)(iVar4 + 0x8de6))) &&
           (*(short *)(iVar4 + 0x8de2) != -1)) goto LAB_006dae08;
        FUN_00648990(*(undefined1 *)(iVar4 + 0x5cc9),iVar15,iVar14);
      }
      else {
        if (iVar16 == -1) {
LAB_006dae2e:
          bVar12 = false;
          if (((0x47 < iVar10) && (iVar10 < 0x6b)) ||
             (((0x8f < iVar10 && (iVar10 < 0xb3)) || (iVar10 - 0x33cU < 0xb3)))) {
            iVar13 = 0x149;
            if ((0x22 < iVar10 - 0x90U) && (iVar13 = 0x147, iVar10 - 0x33cU < 0xb3)) {
              iVar10 = (int)((ulonglong)((longlong)iVar10 * (longlong)DAT_006db304) >> 0x20);
              iVar13 = ((iVar10 >> 3) - (iVar10 >> 0x1f)) + 0x5e6;
            }
            bVar12 = true;
            iVar16 = 0;
            iVar10 = iVar4;
            do {
              if ((*(int *)(iVar10 + 0x94c) == iVar13) && (0 < *(short *)(iVar10 + 0x98a))) {
                if (iVar13 != 0x149) {
                  iVar10 = iVar4 + iVar16 * 0xa0;
                  sVar3 = *(short *)(iVar10 + 0x98a) + -1;
                  *(short *)(iVar10 + 0x98a) = sVar3;
                  if (sVar3 < 1) {
                    FUN_00612a6c(iVar10 + 0x948);
                  }
                }
                FUN_00605890(iVar15,iVar14);
                FUN_006488a4(*(undefined1 *)(iVar4 + 0x5cc9),iVar15,iVar14);
                uVar5 = 1;
                goto LAB_006db286;
              }
              iVar16 = iVar16 + 1;
              iVar10 = iVar10 + 0xa0;
            } while (iVar16 < 0x30);
          }
          if ((bVar12) || (iVar16 = FUN_006056e0(iVar15,iVar14), iVar16 == -1))
          goto switchD_006daf52_caseD_81;
        }
LAB_006daee8:
        if (iVar16 != *(short *)(iVar4 + 0x8de2)) {
          if (*(short *)(iVar4 + 0x8de2) == -1) {
            uVar5 = 10;
          }
          else {
            uVar5 = 0xc;
          }
          FUN_007a7bf4(uVar5);
          *(short *)(iVar4 + 0x8de4) = (short)iVar15;
          *(short *)(iVar4 + 0x8de6) = (short)iVar14;
          *(short *)(iVar4 + 0x8de2) = (short)iVar16;
          FUN_006d0250(iVar4);
          uVar5 = 1;
          goto LAB_006db286;
        }
LAB_006dae08:
        *(undefined2 *)(iVar4 + 0x8de2) = 0xffff;
        FUN_007a7bf4(0xb);
      }
      uVar5 = 1;
      goto LAB_006db286;
    }
  }
  else if (uVar2 == 0x68) {
    FUN_007ab078(auStack_94,&DAT_00879344);
    dVar21 = (double)DAT_010703dc;
    if (DAT_010703e0 == '\0') {
      dVar21 = dVar21 + DAT_006dac84;
    }
    dVar21 = ((dVar21 / DAT_006dac7c) * 24.0 - 7.5) - 12.0;
    if (dVar21 < 0.0) {
      dVar21 = dVar21 + 24.0;
    }
    uVar1 = in_fpscr & 0xfffffff | (uint)(dVar21 < 12.0) << 0x1f;
    uVar17 = uVar1 | (uint)NAN(dVar21) << 0x1c;
    if ((byte)(uVar1 >> 0x1f) == ((byte)(uVar17 >> 0x1c) & 1)) {
      FUN_007aaef0(auStack_94,auStack_274,&DAT_00879348);
      FUN_007aa754(auStack_274);
    }
    iVar15 = (int)(longlong)dVar21;
    dVar18 = (double)VectorSignedToFloat(iVar15,(byte)(uVar17 >> 0x16) & 3);
    dVar21 = (double)VectorSignedToFloat((int)(longlong)((dVar21 - dVar18) * DAT_006dac74),
                                         (byte)(uVar17 >> 0x16) & 3);
    FUN_007ab778(auStack_c4,&DAT_00865e48,(int)(longlong)dVar21);
    if (dVar21 < 10.0) {
      uVar5 = FUN_007ab680(auStack_214,&DAT_0080f438,auStack_c4);
      FUN_007aa80c(auStack_c4,auStack_364,uVar5);
      FUN_007aa754(auStack_364);
      FUN_007aa754(auStack_214);
    }
    if (0xc < iVar15) {
      iVar15 = iVar15 + -0xc;
    }
    if (iVar15 == 0) {
      iVar15 = 0xc;
    }
    uVar5 = FUN_0041c4d8(auStack_394,&DAT_0087934c);
    uVar6 = FUN_0046b9a8();
    piVar7 = (int *)FUN_0049d0e0(uVar6,auStack_37c,uVar5);
    if (7 < (uint)piVar7[5]) {
      piVar7 = (int *)*piVar7;
    }
    FUN_007aada8(auStack_64,piVar7);
    FUN_00423f74(auStack_37c);
    piVar7 = (int *)FUN_007aa600(auStack_94);
    if (0xf < (uint)piVar7[5]) {
      piVar7 = (int *)*piVar7;
    }
    piVar8 = (int *)FUN_007aa600(auStack_c4);
    if (0xf < (uint)piVar8[5]) {
      piVar8 = (int *)*piVar8;
    }
    uVar5 = FUN_007ab778(auStack_f4," %i:%s %s",iVar15,piVar8,piVar7);
    FUN_007aacc4(auStack_64,auStack_1b4,uVar5);
    FUN_007aa754(auStack_1b4);
    FUN_006050e4(auStack_64,0xff,0xf0,0x14,600);
    FUN_007aa754(auStack_64);
    FUN_007aa754(auStack_c4);
    FUN_007aa754(auStack_94);
    uVar5 = 1;
    goto LAB_006db286;
  }
  goto switchD_006daf52_caseD_81;
}



void FUN_00697314(void)

{
  bool bVar1;
  bool bVar2;
  bool bVar3;
  bool bVar4;
  bool bVar5;
  undefined1 uVar6;
  int iVar7;
  int iVar8;
  undefined4 uVar9;
  uint uVar10;
  int iVar11;
  int *piVar12;
  int iVar13;
  int iVar14;
  int iVar15;
  int iVar16;
  int iVar17;
  int iVar18;
  uint in_fpscr;
  float fVar19;
  int local_158;
  int local_154;
  int local_150;
  int local_14c;
  int local_148;
  int local_144;
  int local_140;
  int local_13c;
  int local_138;
  int local_134;
  int local_12c;
  int local_124;
  int local_120;
  int local_118;
  int local_114;
  int local_110;
  int local_10c;
  int local_108;
  int local_104;
  undefined1 local_ec [16];
  undefined4 local_dc;
  undefined4 local_d8;
  undefined2 local_d4 [8];
  undefined4 local_c4;
  undefined4 local_c0;
  undefined4 local_bc;
  undefined4 uStack_b8;
  undefined1 auStack_b4 [48];
  undefined1 auStack_84 [24];
  undefined1 auStack_6c [24];
  undefined1 auStack_54 [48];
  
  FUN_007ffb80();
  local_bc = 0xfffffffe;
  uStack_b8 = 0xfffffffe;
  iVar7 = FUN_007a7d34();
  if ((iVar7 == 0) && (DAT_00fe1f78 = DAT_00fe1f78 + 1, 0x1c1f < DAT_00fe1f78)) {
    DAT_00fe1f78 = 0;
    memset(&DAT_00fee0d0,0,0x405);
    iVar7 = DAT_00697824;
    DAT_00fe1f6c = 0;
    local_158 = 0;
    local_120 = 0;
    local_118 = 0;
    local_140 = 0;
    local_148 = 0;
    iVar8 = 0;
    iVar17 = 0;
    iVar16 = 0;
    iVar18 = 0;
    iVar14 = 0;
    local_110 = 0;
    local_138 = 0;
    local_108 = 0;
    local_12c = 0;
    local_154 = 0;
    local_10c = 0;
    local_124 = 0;
    local_13c = 0;
    local_150 = 0;
    local_144 = 0;
    local_114 = 0;
    local_134 = 0;
    local_104 = 0;
    local_14c = 0;
    iVar13 = 0;
    iVar15 = DAT_00fe1fa8;
    do {
      iVar11 = iVar13 + iVar15;
      if ((*(char *)(iVar11 + 0x100) != '\0') && (*(char *)(iVar11 + 0x118) != '\0')) {
        if ((*(int *)(iVar11 + 0x104) != 0x25) && (*(char *)(iVar11 + 0x119) == '\0')) {
          FUN_0076098c(iVar14);
          iVar8 = local_104;
          iVar15 = DAT_00fe1fa8;
          iVar18 = local_14c;
        }
        iVar11 = *(int *)(iVar13 + iVar15 + 0x104);
        if (iVar11 < 0x26) {
          if (iVar11 == 0x25) {
            iVar8 = iVar8 + 1;
            local_104 = iVar8;
          }
          else {
            switch(iVar11) {
            case 0x11:
              local_158 = local_158 + 1;
              break;
            case 0x12:
              local_120 = local_120 + 1;
              break;
            case 0x13:
              local_140 = local_140 + 1;
              break;
            case 0x14:
              local_118 = local_118 + 1;
              break;
            case 0x16:
              local_148 = local_148 + 1;
            }
          }
        }
        else if (iVar11 < 0xb3) {
          if (iVar11 == 0xb2) {
            local_10c = local_10c + 1;
          }
          else if (iVar11 < 0x6d) {
            if (iVar11 == 0x6c) {
              local_110 = local_110 + 1;
            }
            else if (iVar11 == 0x26) {
              iVar17 = iVar17 + 1;
            }
            else if (iVar11 == 0x36) {
              iVar16 = iVar16 + 1;
            }
            else if (iVar11 == 0x6b) {
              local_138 = local_138 + 1;
            }
          }
          else if (iVar11 == 0x7c) {
            local_108 = local_108 + 1;
          }
          else if (iVar11 == 0x8e) {
            local_12c = local_12c + 1;
          }
          else if (iVar11 == 0xa0) {
            local_154 = local_154 + 1;
          }
        }
        else {
          switch(iVar11) {
          case 0xcf:
            local_124 = local_124 + 1;
            break;
          case 0xd0:
            local_13c = local_13c + 1;
            break;
          case 0xd1:
            local_150 = local_150 + 1;
            break;
          case 0xe3:
            local_144 = local_144 + 1;
            break;
          case 0xe4:
            local_114 = local_114 + 1;
            break;
          case 0xe5:
            local_134 = local_134 + 1;
          }
        }
        iVar18 = iVar18 + 1;
        local_14c = iVar18;
      }
      iVar13 = iVar13 + 0x274;
      iVar14 = iVar14 + 1;
    } while (iVar13 < iVar7);
    if (DAT_00fe1f6c == 0) {
      bVar2 = false;
      iVar7 = 0;
      bVar3 = false;
      bVar4 = false;
      bVar5 = false;
      iVar8 = 4;
      piVar12 = &DAT_0107034c;
      do {
        iVar15 = *piVar12;
        if (*(char *)(iVar15 + 0x5cc5) != '\0') {
          iVar13 = 0x30;
          iVar18 = iVar15;
          do {
            iVar14 = *(int *)(iVar18 + 0x94c);
            if ((0 < iVar14) && (iVar11 = (int)*(short *)(iVar18 + 0x98a), 0 < iVar11)) {
              if (iVar14 == 0x47) {
                iVar7 = iVar7 + iVar11;
              }
              else if (iVar14 == 0x48) {
                iVar7 = iVar11 * 100 + iVar7;
              }
              else if (iVar14 == 0x49) {
                iVar7 = iVar11 * 10000 + iVar7;
              }
              else if (iVar14 == 0x4a) {
                iVar7 = iVar11 * DAT_00697820 + iVar7;
              }
              if ((*(short *)(iVar18 + 0x9ce) == 0xe) || (*(short *)(iVar18 + 0x9d0) == 0xe)) {
                bVar4 = true;
              }
              if ((((iVar14 == 0xa6) || (iVar14 == 0xa7)) || (iVar14 == 0xa8)) || (iVar14 == 0xeb))
              {
                bVar5 = true;
              }
              if ((*(char *)(iVar18 + 0x97f) != '\0') || (iVar14 - 0x453U < 0xe)) {
                bVar2 = true;
              }
            }
            iVar18 = iVar18 + 0xa0;
            iVar13 = iVar13 + -1;
          } while (iVar13 != 0);
          iVar18 = (int)((ulonglong)
                         ((longlong)(int)*(short *)(iVar15 + 0x5d00) * (longlong)DAT_0069781c) >>
                        0x20);
          if (5 < (iVar18 >> 3) - (iVar18 >> 0x1f)) {
            bVar3 = true;
          }
          if (!bVar2) {
            if ((0 < *(short *)(iVar15 + 0x282a)) && (*(char *)(iVar15 + 0x281f) != '\0')) {
              bVar2 = true;
            }
            if ((0 < *(short *)(iVar15 + 0x28ca)) && (*(char *)(iVar15 + 0x28bf) != '\0')) {
              bVar2 = true;
            }
            if ((0 < *(short *)(iVar15 + 0x296a)) && (*(char *)(iVar15 + 0x295f) != '\0')) {
              bVar2 = true;
            }
          }
        }
        iVar8 = iVar8 + -1;
        piVar12 = piVar12 + 1;
      } while (iVar8 != 0);
      if ((DAT_00fe1f7b == '\0') && (local_104 == 0)) {
        iVar15 = 0xc4;
        iVar18 = (int)DAT_01033964;
        iVar13 = (int)DAT_01033968;
        iVar14 = 0;
        iVar8 = DAT_00fe1fa8;
        do {
          if (*(char *)(iVar8 + 0x100) == '\0') {
            iVar15 = iVar14;
            if (iVar14 < 0xc4) {
              FUN_00684770(iVar14 * 0x274 + DAT_00fe1fa8,0x25);
              iVar8 = iVar14 * 0x274 + DAT_00fe1fa8;
              iVar13 = (iVar13 * 0x10 + 8) - (uint)(*(ushort *)(iVar8 + 0x168) >> 1);
              *(int *)(iVar8 + 0x148) = iVar13;
              uVar9 = VectorSignedToFloat(iVar13,(byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar8 + 0x138) = uVar9;
              iVar8 = iVar14 * 0x274 + DAT_00fe1fa8;
              iVar18 = iVar18 * 0x10 - (uint)*(ushort *)(iVar8 + 0x16a);
              *(int *)(iVar8 + 0x14c) = iVar18;
              uVar9 = VectorSignedToFloat(iVar18,(byte)(in_fpscr >> 0x16) & 3);
              *(undefined4 *)(iVar8 + 0x13c) = uVar9;
              *(undefined1 *)(iVar14 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
              *(undefined4 *)(iVar14 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
              iVar8 = iVar14 * 0x274 + DAT_00fe1fa8;
              uVar6 = FUN_0060a19c(iVar8 + 0x138,*(undefined2 *)(iVar8 + 0x168),
                                   *(undefined2 *)(iVar8 + 0x16a));
              *(undefined1 *)(iVar14 * 0x274 + DAT_00fe1fa8 + 100) = uVar6;
              FUN_00649e08(iVar14,1);
            }
            break;
          }
          iVar14 = iVar14 + 1;
          iVar8 = iVar8 + 0x274;
        } while (iVar14 < 0xc4);
        *(undefined1 *)(iVar15 * 0x274 + DAT_00fe1fa8 + 0x119) = 0;
        *(short *)(iVar15 * 0x274 + DAT_00fe1fa8 + 0x1f2) = DAT_01033968;
        *(short *)(iVar15 * 0x274 + DAT_00fe1fa8 + 500) = DAT_01033964;
      }
      uVar10 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
      DAT_00fd67f4 = DAT_00fd67f8;
      DAT_00fd67f8 = DAT_00fd67fc;
      uVar10 = DAT_00fd6800 ^ (uVar10 ^ DAT_00fd6800 >> 0xb) >> 8 ^ uVar10;
      DAT_00fd67fc = DAT_00fd6800;
      fVar19 = (float)VectorSignedToFloat(uVar10 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
      bVar1 = (int)(fVar19 * DAT_00697818 * DAT_00697814) != 0;
      if (local_148 < 1) {
        DAT_00fee0e6 = 1;
      }
      if ((5000 < iVar7) && (local_158 < 1)) {
        DAT_00fee0e1 = 1;
      }
      if (((bVar3) && (local_120 < 1)) && (0 < local_158)) {
        DAT_00fee0e2 = 1;
      }
      if ((bVar4) && (local_140 < 1)) {
        DAT_00fee0e3 = 1;
      }
      if ((((DAT_00fe1f73 != '\0') || (DAT_00fe1f7a != '\0')) || (DAT_00fe1f7b != '\0')) &&
         (local_118 < 1)) {
        DAT_00fee0e4 = 1;
      }
      if (((bVar5) && (0 < local_158)) && (iVar17 < 1)) {
        DAT_00fee0f6 = 1;
      }
      if ((DAT_00fe1f7b != '\0') && (iVar16 < 1)) {
        DAT_00fee106 = 1;
      }
      if ((DAT_00fe1f7e != '\0') && (local_138 < 1)) {
        DAT_00fee13b = 1;
      }
      if ((DAT_00fe1f7f != '\0') && (local_110 < 1)) {
        DAT_00fee13c = 1;
      }
      if ((DAT_00fe1f82 != '\0') && (local_108 < 1)) {
        DAT_00fee14c = 1;
      }
      DAT_00fd6800 = uVar10;
      if (((DAT_00fe1f84 != '\0') && (local_12c < 1)) && (iVar8 = FUN_0073542c(), iVar8 != 0)) {
        DAT_00fee15e = 1;
      }
      if ((DAT_00fe1f8b != '\0') && (local_10c < 1)) {
        DAT_00fee182 = 1;
      }
      if ((bVar2) && (local_124 < 1)) {
        DAT_00fee19f = 1;
      }
      if ((DAT_00fe1f8c != '\0') && (local_114 < 1)) {
        DAT_00fee1b4 = 1;
      }
      if ((DAT_00fe1f87 != '\0') && (local_134 < 1)) {
        DAT_00fee1b5 = 1;
      }
      if ((local_154 < 1) && (DAT_010338d3 != '\0')) {
        DAT_00fee170 = 1;
      }
      if (((DAT_010338d3 != '\0') && (DAT_00fe1f8d != '\0')) && (local_150 < 1)) {
        DAT_00fee1a1 = 1;
      }
      if ((3 < local_14c) && (local_144 < 1)) {
        DAT_00fee1b3 = 1;
      }
      if (((!bVar1) && (local_13c < 1)) && (7 < local_14c)) {
        DAT_00fee1a0 = 1;
      }
      if ((local_148 < 1) && (DAT_00fee0e6 = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0x16;
      }
      if (((5000 < iVar7) && (local_158 < 1)) && (DAT_00fee0e1 = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0x11;
      }
      if (((bVar3) && (local_120 < 1)) && ((0 < local_158 && (DAT_00fee0e2 = 1, DAT_00fe1f6c == 0)))
         ) {
        DAT_00fe1f6c = 0x12;
      }
      if (((bVar4) && (local_140 < 1)) && (DAT_00fee0e3 = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0x13;
      }
      if ((local_118 < 1) &&
         ((((DAT_00fe1f73 != '\0' || (DAT_00fe1f7a != '\0')) || (DAT_00fe1f7b != '\0')) &&
          (DAT_00fee0e4 = 1, DAT_00fe1f6c == 0)))) {
        DAT_00fe1f6c = 0x14;
      }
      if (((bVar5) && (0 < local_158)) && ((iVar17 < 1 && (DAT_00fee0f6 = 1, DAT_00fe1f6c == 0)))) {
        DAT_00fe1f6c = 0x26;
      }
      if (((DAT_00fe1f7b != '\0') && (iVar16 < 1)) && (DAT_00fee106 = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0x36;
      }
      if (((DAT_00fe1f7e != '\0') && (local_138 < 1)) && (DAT_00fee13b = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0x6b;
      }
      if (((DAT_00fe1f7f != '\0') && (local_110 < 1)) && (DAT_00fee13c = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0x6c;
      }
      if (((DAT_00fe1f82 != '\0') && (local_108 < 1)) && (DAT_00fee14c = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0x7c;
      }
      if ((((DAT_00fe1f84 != '\0') && (local_12c < 1)) && (iVar7 = FUN_0073542c(), iVar7 != 0)) &&
         (DAT_00fee15e = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0x8e;
      }
      if (((DAT_00fe1f8b != '\0') && (local_10c < 1)) && (DAT_00fee182 = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0xb2;
      }
      if (((bVar2) && (local_124 < 1)) && (DAT_00fee19f = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0xcf;
      }
      if (((DAT_00fe1f8c != '\0') && (local_114 < 1)) && (DAT_00fee1b4 = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0xe4;
      }
      if (((DAT_00fe1f87 != '\0') && (local_134 < 1)) && (DAT_00fee1b5 = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0xe5;
      }
      if (DAT_010338d3 != '\0') {
        if ((local_154 < 1) && (DAT_00fee170 = 1, DAT_00fe1f6c == 0)) {
          DAT_00fe1f6c = 0xa0;
        }
        if ((((DAT_010338d3 != '\0') && (DAT_00fe1f8d != '\0')) && (local_150 < 1)) &&
           (DAT_00fee1a1 = 1, DAT_00fe1f6c == 0)) {
          DAT_00fe1f6c = 0xd1;
        }
      }
      if (((3 < local_14c) && (local_144 < 1)) && (DAT_00fee1b3 = 1, DAT_00fe1f6c == 0)) {
        DAT_00fe1f6c = 0xe3;
      }
      if (((bVar1) || (local_14c < 8)) || (0 < local_13c)) {
        if (DAT_00fe1f6c == 0) goto LAB_00697cc4;
      }
      else {
        DAT_00fee1a0 = 1;
        if (DAT_00fe1f6c == 0) {
          DAT_00fe1f6c = 0xd0;
        }
      }
    }
    FUN_007ab35c(auStack_54,&DAT_00821498,&DAT_00fdb658);
    FUN_007ab078(auStack_84,&DAT_0080ed7b);
    uVar9 = FUN_007ab4d4(&DAT_00fe1fb0 + DAT_00fe1f6c * 0x30,auStack_b4,auStack_54);
    FUN_007aa80c(auStack_84,local_ec,uVar9);
    FUN_007aa754(local_ec);
    FUN_007aa754(auStack_b4);
    local_d8 = 0xf;
    local_dc = 0;
    local_ec[0] = 0;
    FUN_0041bae0(local_ec,auStack_84,0,0xffffffff);
    local_c0 = 7;
    local_c4 = 0;
    local_d4[0] = 0;
    FUN_00425168(local_d4,auStack_6c,0,0xffffffff);
    FUN_00646c1c(local_ec,0xff,0xff,0xff,0xffffffff);
    FUN_007aa754(auStack_84);
    FUN_007aa754(auStack_54);
  }
LAB_00697cc4:
  FUN_007ffb98();
  return;
}


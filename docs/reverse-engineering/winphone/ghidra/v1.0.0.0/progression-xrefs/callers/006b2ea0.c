
int FUN_006b2ea0(int *param_1)

{
  char cVar1;
  float fVar2;
  char cVar3;
  undefined1 uVar4;
  int iVar5;
  uint uVar6;
  undefined4 uVar7;
  uint uVar8;
  uint uVar9;
  int iVar10;
  int iVar11;
  undefined4 uVar12;
  char cVar13;
  int iVar14;
  int iVar15;
  int iVar16;
  uint in_fpscr;
  float fVar17;
  uint local_2f8;
  uint local_2f0;
  int local_2e8;
  int local_2e4;
  undefined1 auStack_2d8 [48];
  undefined1 auStack_2a8 [48];
  undefined4 local_278;
  undefined4 uStack_274;
  undefined1 auStack_270 [48];
  undefined1 auStack_240 [48];
  undefined1 auStack_210 [48];
  undefined1 auStack_1e0 [48];
  undefined1 auStack_1b0 [48];
  undefined1 auStack_180 [48];
  undefined1 auStack_150 [48];
  undefined1 auStack_120 [48];
  undefined1 auStack_f0 [48];
  undefined1 auStack_c0 [48];
  undefined1 auStack_90 [48];
  undefined1 auStack_60 [60];
  
  fVar2 = DAT_006b30cc;
  local_278 = 0xfffffffe;
  uStack_274 = 0xfffffffe;
  iVar10 = param_1[2];
  iVar14 = *param_1;
  iVar15 = param_1[1];
  iVar11 = param_1[3];
  iVar16 = param_1[5];
  local_2e4 = (int)DAT_00902ea4;
  if (DAT_00fe1f83 == '\0') {
    local_2f8 = DAT_00fd6800;
    local_2f0 = DAT_00fd67f4;
  }
  else {
    uVar6 = DAT_00fd67f4 ^ DAT_00fd67f4 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    local_2f0 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    local_2f8 = uVar6 ^ (uVar6 ^ DAT_00fd6800 >> 0xb) >> 8 ^ DAT_00fd6800;
    DAT_00fd67fc = DAT_00fd6800;
    fVar17 = (float)VectorSignedToFloat(local_2f8 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd6800 = local_2f8;
    if ((((((int)(fVar17 * DAT_006b30cc * 20.0) == 0) && (*(char *)((int)param_1 + 0x11) == '\0'))
         && (DAT_01033960 <= param_1[3])) &&
        ((param_1[3] < local_2e4 + -0xd2 && (DAT_00fe1f7e == '\0')))) &&
       (iVar5 = FUN_00656370(0x69), iVar5 == 0)) {
      uVar12 = 0;
      uVar7 = 0x69;
      goto LAB_006b490e;
    }
  }
  cVar13 = DAT_010338d3;
  uVar6 = local_2f8;
  if (DAT_010338d3 != '\0') {
    uVar6 = local_2f0 ^ local_2f0 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    local_2f0 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    uVar6 = uVar6 ^ (uVar6 ^ local_2f8 >> 0xb) >> 8 ^ local_2f8;
    fVar17 = (float)VectorSignedToFloat(uVar6 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    DAT_00fd67fc = local_2f8;
    DAT_00fd6800 = uVar6;
    if ((((int)(fVar17 * fVar2 * 20.0) == 0) && (*(char *)((int)param_1 + 0x11) == '\0')) &&
       ((DAT_01033960 <= param_1[3] &&
        (((param_1[3] < local_2e4 + -0xd2 && (DAT_00fe1f7f == '\0')) &&
         (iVar5 = FUN_00656370(0x6a), iVar5 == 0)))))) {
      uVar12 = 0;
      uVar7 = 0x6a;
      goto LAB_006b490e;
    }
  }
  local_2f8 = uVar6;
  cVar1 = DAT_010703e0;
  uVar9 = DAT_00fd67fc;
  uVar6 = DAT_00fd67f8;
  local_2e8 = DAT_00902ebc;
  uVar8 = local_2f8;
  if ((DAT_01033966 != '\0') && (DAT_00902ebc < iVar11)) {
    uVar8 = local_2f0 ^ local_2f0 << 0xb;
    local_2f0 = DAT_00fd67f8;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd6800 = uVar8 ^ (uVar8 ^ local_2f8 >> 0xb) >> 8 ^ local_2f8;
    DAT_00fd67fc = local_2f8;
    fVar17 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    uVar8 = DAT_00fd6800;
    if ((int)(fVar17 * fVar2 * DAT_006b30c8) == 0) {
      iVar11 = 0;
      iVar10 = DAT_00fe1fa8;
      while (uVar8 = DAT_00fd67fc, *(char *)(iVar10 + 0x100) != '\0') {
        iVar11 = iVar11 + 1;
        iVar10 = iVar10 + 0x274;
        if (0xc3 < iVar11) {
          DAT_00fd67f4 = uVar6;
          DAT_00fd67f8 = uVar9;
          return 0xc4;
        }
      }
      goto LAB_006b3176;
    }
  }
  local_2f8 = uVar8;
  uVar6 = DAT_00fd67f8;
  uVar9 = local_2f8;
  uVar8 = DAT_00fd67fc;
  if ((cVar13 != '\0') &&
     (*(char *)((DAT_00902934 * iVar10 + iVar11 + -1) * 0xe + DAT_00902928 + 8) == '\x02')) {
    uVar9 = local_2f0 ^ local_2f0 << 0xb;
    local_2f0 = DAT_00fd67f8;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd6800 = local_2f8 ^ (uVar9 ^ local_2f8 >> 0xb) >> 8 ^ uVar9;
    fVar17 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    uVar9 = DAT_00fd6800;
    uVar8 = local_2f8;
    if ((int)(fVar17 * fVar2 * 20.0) == 0) {
      iVar11 = 0;
      iVar10 = DAT_00fe1fa8;
      while (uVar8 = local_2f8, *(char *)(iVar10 + 0x100) != '\0') {
        iVar11 = iVar11 + 1;
        iVar10 = iVar10 + 0x274;
        if (0xc3 < iVar11) {
          DAT_00fd67f4 = uVar6;
          DAT_00fd67fc = local_2f8;
          return 0xc4;
        }
      }
LAB_006b3176:
      DAT_00fd67fc = uVar8;
      if (0xc3 < iVar11) {
        return iVar11;
      }
      FUN_00684770(iVar11 * 0x274 + DAT_00fe1fa8,0x55);
      iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
      iVar14 = iVar14 - (uint)(*(ushort *)(iVar10 + 0x168) >> 1);
      *(int *)(iVar10 + 0x148) = iVar14;
      uVar7 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
      *(undefined4 *)(iVar10 + 0x138) = uVar7;
      iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
      iVar15 = iVar15 - (uint)*(ushort *)(iVar10 + 0x16a);
      *(int *)(iVar10 + 0x14c) = iVar15;
      uVar7 = VectorSignedToFloat(iVar15,(byte)(in_fpscr >> 0x16) & 3);
      *(undefined4 *)(iVar10 + 0x13c) = uVar7;
      *(undefined1 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
      *(undefined4 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
      iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
      uVar4 = FUN_0060a19c(iVar10 + 0x138,*(undefined2 *)(iVar10 + 0x168),
                           *(undefined2 *)(iVar10 + 0x16a));
      *(undefined1 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 100) = uVar4;
      FUN_00649e08(iVar11,1);
      return iVar11;
    }
  }
  DAT_00fd67fc = uVar8;
  if (cVar13 != '\0') {
    if ((iVar11 <= DAT_00902ebc) && (DAT_010703e0 == '\0')) {
      iVar5 = FUN_00608330(&DAT_00fd67f4,0x14);
      if ((iVar5 == 0) ||
         ((iVar5 = FUN_00608330(&DAT_00fd67f4,5), iVar5 == 0 && (DAT_010703e4 == '\x04')))) {
        iVar11 = 0;
        iVar10 = DAT_00fe1fa8;
        do {
          if (*(char *)(iVar10 + 0x100) == '\0') {
            if (0xc3 < iVar11) {
              return iVar11;
            }
            FUN_00684770(iVar11 * 0x274 + DAT_00fe1fa8,0x52);
            iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
            iVar14 = iVar14 - (uint)(*(ushort *)(iVar10 + 0x168) >> 1);
            *(int *)(iVar10 + 0x148) = iVar14;
            uVar7 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
            *(undefined4 *)(iVar10 + 0x138) = uVar7;
            iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
            iVar15 = iVar15 - (uint)*(ushort *)(iVar10 + 0x16a);
            *(int *)(iVar10 + 0x14c) = iVar15;
            uVar7 = VectorSignedToFloat(iVar15,(byte)(in_fpscr >> 0x16) & 3);
            *(undefined4 *)(iVar10 + 0x13c) = uVar7;
            *(undefined1 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
            *(undefined4 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
            iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
            uVar4 = FUN_0060a19c(iVar10 + 0x138,*(undefined2 *)(iVar10 + 0x168),
                                 *(undefined2 *)(iVar10 + 0x16a));
            *(undefined1 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 100) = uVar4;
            FUN_00649e08(iVar11,1);
            return iVar11;
          }
          iVar11 = iVar11 + 1;
          iVar10 = iVar10 + 0x274;
        } while (iVar11 < 0xc4);
        return 0xc4;
      }
      local_2f0 = DAT_00fd67f4;
      uVar9 = DAT_00fd6800;
    }
    if (cVar13 != '\0') {
      iVar5 = FUN_007354a8();
      cVar1 = DAT_010703e0;
      cVar13 = DAT_010338d3;
      local_2f0 = DAT_00fd67f4;
      uVar9 = DAT_00fd6800;
      if (iVar5 == 0) {
        local_2e4 = (int)DAT_00902ea4;
        local_2e8 = DAT_00902ebc;
      }
      else {
        local_2e8 = DAT_00902ebc;
        if (DAT_00902ebc < iVar11) {
          local_2e4 = (int)DAT_00902ea4;
        }
        else {
          if ((DAT_010703e0 == '\0') && (iVar5 = FUN_00608330(&DAT_00fd67f4,10), iVar5 == 0)) {
            iVar11 = 0;
            iVar10 = DAT_00fe1fa8;
            do {
              if (*(char *)(iVar10 + 0x100) == '\0') {
                if (0xc3 < iVar11) {
                  return iVar11;
                }
                FUN_00684770(iVar11 * 0x274 + DAT_00fe1fa8,0x130);
                iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
                iVar14 = iVar14 - (uint)(*(ushort *)(iVar10 + 0x168) >> 1);
                *(int *)(iVar10 + 0x148) = iVar14;
                uVar7 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
                *(undefined4 *)(iVar10 + 0x138) = uVar7;
                iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
                iVar15 = iVar15 - (uint)*(ushort *)(iVar10 + 0x16a);
                *(int *)(iVar10 + 0x14c) = iVar15;
                uVar7 = VectorSignedToFloat(iVar15,(byte)(in_fpscr >> 0x16) & 3);
                *(undefined4 *)(iVar10 + 0x13c) = uVar7;
                *(undefined1 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
                *(undefined4 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
                iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
                uVar4 = FUN_0060a19c(iVar10 + 0x138,*(undefined2 *)(iVar10 + 0x168),
                                     *(undefined2 *)(iVar10 + 0x16a));
                *(undefined1 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 100) = uVar4;
                FUN_00649e08(iVar11,1);
                return iVar11;
              }
              iVar11 = iVar11 + 1;
              iVar10 = iVar10 + 0x274;
            } while (iVar11 < 0xc4);
            return 0xc4;
          }
          local_2e4 = (int)DAT_00902ea4;
          local_2f0 = DAT_00fd67f4;
          uVar9 = DAT_00fd6800;
          cVar13 = DAT_010338d3;
        }
      }
    }
  }
  uVar6 = DAT_00fd67f8;
  if (iVar16 == 0x3c) {
    local_2f0 = local_2f0 ^ local_2f0 << 0xb;
    DAT_00fd67f4 = DAT_00fd67f8;
    DAT_00fd67f8 = DAT_00fd67fc;
    DAT_00fd6800 = uVar9 ^ (local_2f0 ^ uVar9 >> 0xb) >> 8 ^ local_2f0;
    fVar17 = (float)VectorSignedToFloat(DAT_00fd6800 & 0x7fffffff,(byte)(in_fpscr >> 0x16) & 3);
    if (((int)(fVar17 * fVar2 * DAT_006b3854) == 0) && (cVar1 == '\0')) {
      iVar11 = 0;
      iVar10 = DAT_00fe1fa8;
      do {
        if (*(char *)(iVar10 + 0x100) == '\0') {
          if (0xc3 < iVar11) {
            DAT_00fd67f4 = uVar6;
            DAT_00fd67fc = uVar9;
            return iVar11;
          }
          DAT_00fd67fc = uVar9;
          FUN_00684770(iVar11 * 0x274 + DAT_00fe1fa8,0x34);
          iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
          iVar14 = iVar14 - (uint)(*(ushort *)(iVar10 + 0x168) >> 1);
          *(int *)(iVar10 + 0x148) = iVar14;
          uVar7 = VectorSignedToFloat(iVar14,(byte)(in_fpscr >> 0x16) & 3);
          *(undefined4 *)(iVar10 + 0x138) = uVar7;
          iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
          iVar15 = iVar15 - (uint)*(ushort *)(iVar10 + 0x16a);
          *(int *)(iVar10 + 0x14c) = iVar15;
          uVar7 = VectorSignedToFloat(iVar15,(byte)(in_fpscr >> 0x16) & 3);
          *(undefined4 *)(iVar10 + 0x13c) = uVar7;
          *(undefined1 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
          *(undefined4 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
          iVar10 = iVar11 * 0x274 + DAT_00fe1fa8;
          uVar4 = FUN_0060a19c(iVar10 + 0x138,*(undefined2 *)(iVar10 + 0x168),
                               *(undefined2 *)(iVar10 + 0x16a));
          *(undefined1 *)(iVar11 * 0x274 + DAT_00fe1fa8 + 100) = uVar4;
          FUN_00649e08(iVar11,1);
          return iVar11;
        }
        iVar11 = iVar11 + 1;
        iVar10 = iVar10 + 0x274;
      } while (iVar11 < 0xc4);
      DAT_00fd67f4 = uVar6;
      DAT_00fd67fc = uVar9;
      return 0xc4;
    }
    DAT_00fd67fc = uVar9;
    if (local_2e8 < iVar11) {
      iVar5 = FUN_00608330(&DAT_00fd67f4,0x3c);
      if (iVar5 == 0) {
        uVar12 = 0;
        uVar7 = 0xdb;
        goto LAB_006b490e;
      }
      goto LAB_006b3634;
    }
  }
  else {
LAB_006b3634:
    if ((((local_2e8 < iVar11) && (iVar11 < local_2e4 + -0xd2)) &&
        (iVar5 = param_1[6], *(char *)((&DAT_0107034c)[iVar5] + 0xea) == '\0')) &&
       (((*(char *)((&DAT_0107034c)[iVar5] + 0xeb) == '\0' &&
         (*(char *)((&DAT_0107034c)[iVar5] + 0xe6) == '\0')) &&
        ((*(char *)((&DAT_0107034c)[iVar5] + 0xe9) == '\0' &&
         ((*(char *)((&DAT_0107034c)[iVar5] + 0xe7) == '\0' &&
          (iVar5 = FUN_00608330(&DAT_00fd67f4,10), iVar5 == 0)))))))) {
      uVar7 = 300;
      uVar12 = 0;
      goto LAB_006b490e;
    }
  }
  if (((cVar13 != '\0') && (iVar16 == 0x35)) && (iVar5 = FUN_00608330(&DAT_00fd67f4,3), iVar5 == 0))
  {
    uVar12 = 0;
    uVar7 = 0x4e;
    goto LAB_006b490e;
  }
  if (param_1[3] <= local_2e8) {
    if (cVar1 != '\0') {
      iVar11 = FUN_006abb14(param_1);
      cVar13 = DAT_010299a0;
      if (-1 < iVar11) {
        return iVar11;
      }
      if ((iVar16 == 0x35) && (*(char *)((int)param_1 + 0x11) == '\0')) {
        iVar10 = FUN_00608330(&DAT_00fd67f4,5);
        if (iVar10 == 0) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,2);
          if (iVar10 == 0) {
            uVar7 = 0x45;
          }
          else {
            uVar7 = 0x3e9;
          }
          uVar12 = 0;
        }
        else {
          uVar12 = 0;
          uVar7 = 0x3d;
        }
      }
      else {
        if (DAT_010299a0 == '\0') {
LAB_006b3734:
          iVar11 = FUN_00685550(iVar14,iVar15,1,0);
          uVar9 = iVar10 - DAT_010338cc;
          uVar6 = (int)uVar9 >> 0x1f;
          iVar10 = (uVar9 ^ uVar6) - uVar6;
          if (iVar16 == 0x3c) {
            FUN_007ab078(auStack_2a8,"Jungle Slime");
            FUN_0067a84c(iVar11 * 0x274 + DAT_00fe1fa8,auStack_2a8);
            FUN_007aa754(auStack_2a8);
            return iVar11;
          }
          if ((iVar16 == 0xa1) || (iVar16 == 0x93)) {
            FUN_0066c0b8(0xbf800000,iVar11 * 0x274 + DAT_00fe1fa8,0x93);
            return iVar11;
          }
          iVar14 = FUN_007354a8();
          if ((iVar14 != 0) && (iVar14 = FUN_00608330(&DAT_00fd67f4,3), iVar14 != 0)) {
            FUN_0066c0b8(0xbf800000,iVar11 * 0x274 + DAT_00fe1fa8,0x12e);
            return iVar11;
          }
          iVar15 = FUN_00608330(&DAT_00fd67f4,3);
          iVar14 = DAT_006b3850;
          if ((iVar15 != 0) &&
             (iVar16 = (int)DAT_00902ea0,
             iVar15 = (int)((ulonglong)((longlong)iVar16 * (longlong)DAT_006b3850) >> 0x20),
             (iVar15 >> 2) - (iVar15 >> 0x1f) <= iVar10)) {
            iVar15 = FUN_00608330(&DAT_00fd67f4,10);
            if (iVar15 != 0) {
              return iVar11;
            }
            iVar14 = (int)((ulonglong)((longlong)iVar16 * (longlong)iVar14) >> 0x20);
            if (iVar10 <= (iVar14 >> 1) - (iVar14 >> 0x1f)) {
              return iVar11;
            }
            FUN_007ab078(auStack_2a8,"Purple Slime");
            FUN_0067a84c(iVar11 * 0x274 + DAT_00fe1fa8,auStack_2a8);
            FUN_007aa754(auStack_2a8);
            return iVar11;
          }
          FUN_007ab078(auStack_2a8,"Green Slime");
          FUN_0067a84c(iVar11 * 0x274 + DAT_00fe1fa8,auStack_2a8);
          FUN_007aa754(auStack_2a8);
          return iVar11;
        }
        iVar11 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar11 == 0) {
          uVar12 = 0;
          uVar7 = 0xe0;
        }
        else {
          if ((cVar13 == '\0') || (iVar11 = FUN_00608330(&DAT_00fd67f4,2), iVar11 != 0))
          goto LAB_006b3734;
          uVar12 = 0;
          uVar7 = 0xe1;
        }
      }
      goto LAB_006b490e;
    }
    iVar10 = FUN_00608330(&DAT_00fd67f4,10);
    if ((iVar10 == 0) && (iVar10 = FUN_007354a8(), cVar13 = DAT_010338d3, iVar10 != 0)) {
      uVar7 = 0x12d;
      uVar12 = 0;
      goto LAB_006b490e;
    }
    iVar10 = FUN_00608330(&DAT_00fd67f4,6);
    cVar1 = DAT_010703e4;
    if ((iVar10 == 0) ||
       ((DAT_010703e4 == '\x04' && (iVar10 = FUN_00608330(&DAT_00fd67f4,2), iVar10 == 0)))) {
      if ((cVar13 == '\0') || (iVar10 = FUN_00608330(&DAT_00fd67f4,3), iVar10 != 0)) {
        iVar10 = FUN_007354a8();
        if ((iVar10 == 0) || (iVar10 = FUN_00608330(&DAT_00fd67f4,2), iVar10 != 0)) {
          uVar7 = FUN_00608330(&DAT_00fd67f4,10);
          switch(uVar7) {
          case 0:
            goto switchD_006b3ed8_caseD_0;
          case 1:
            iVar10 = FUN_00685550(iVar14,iVar15,0xbf,0);
            iVar11 = FUN_00608330(&DAT_00fd67f4,3);
            if (iVar11 != 0) {
              return iVar10;
            }
            FUN_007ab078(auStack_1b0,"Sleepy Eye 2");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_1b0);
            FUN_007aa754(auStack_1b0);
            return iVar10;
          case 2:
            iVar10 = FUN_00685550(iVar14,iVar15,0xc0,0);
            iVar11 = FUN_00608330(&DAT_00fd67f4,3);
            if (iVar11 != 0) {
              return iVar10;
            }
            FUN_007ab078(auStack_210,"Dialated Eye 2");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_210);
            FUN_007aa754(auStack_210);
            return iVar10;
          case 3:
            iVar10 = FUN_00685550(iVar14,iVar15,0xc1,0);
            iVar11 = FUN_00608330(&DAT_00fd67f4,3);
            if (iVar11 != 0) {
              return iVar10;
            }
            FUN_007ab078(auStack_270,"Green Eye 2");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_270);
            FUN_007aa754(auStack_270);
            return iVar10;
          case 4:
            iVar10 = FUN_00685550(iVar14,iVar15,0xc2,0);
            iVar11 = FUN_00608330(&DAT_00fd67f4,3);
            if (iVar11 != 0) {
              return iVar10;
            }
            FUN_007ab078(auStack_1e0,"Purple Eye 2");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_1e0);
            FUN_007aa754(auStack_1e0);
            return iVar10;
          default:
            iVar10 = FUN_00685550(iVar14,iVar15,2,0);
            iVar11 = FUN_00608330(&DAT_00fd67f4,4);
            if (iVar11 != 0) {
              return iVar10;
            }
            FUN_007ab078(auStack_240,"Demon Eye 2");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_240);
            FUN_007aa754(auStack_240);
            return iVar10;
          }
        }
        uVar7 = FUN_005c4e6c(&DAT_00fd67f4,0x13d,0x13f);
        uVar12 = 0;
      }
      else {
        uVar12 = 0;
        uVar7 = 0x85;
      }
      goto LAB_006b490e;
    }
    cVar3 = DAT_010703e2;
    if ((((DAT_01033966 != '\0') && (DAT_010703e2 != '\0')) &&
        (iVar10 = FUN_00608330(&DAT_00fd67f4,0x32), iVar10 == 0)) &&
       (iVar10 = FUN_00656370(0x6d), iVar10 == 0)) {
      uVar12 = 0;
      uVar7 = 0x6d;
      goto LAB_006b490e;
    }
    iVar10 = FUN_00608330(&DAT_00fd67f4,0x96);
    if ((iVar10 == 0) && (cVar3 != '\0')) {
      uVar12 = 0;
      uVar7 = 0x35;
      goto LAB_006b490e;
    }
    if (cVar1 == '\0') {
      if (cVar13 != '\0') {
        iVar10 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar10 != 0) {
          uVar12 = 0;
          uVar7 = 0x68;
          goto LAB_006b490e;
        }
        goto LAB_006b3964;
      }
    }
    else {
LAB_006b3964:
      if ((cVar13 != '\0') && (iVar10 = FUN_00608330(&DAT_00fd67f4,3), iVar10 == 0))
      goto LAB_006b3970;
    }
    if ((((iVar16 != 0x93) && (iVar16 != 0xa1)) && (iVar16 != 0xa3)) &&
       ((iVar16 != 0xa4 && (iVar16 != 0xa2)))) {
      if ((DAT_010299a0 != '\0') && (iVar10 = FUN_00608330(&DAT_00fd67f4,2), iVar10 == 0)) {
        iVar10 = FUN_00685550(iVar14,iVar15,0xdf,0);
        iVar11 = FUN_00608330(&DAT_00fd67f4,3);
        if (iVar11 != 0) {
          return iVar10;
        }
        iVar11 = FUN_00608330(&DAT_00fd67f4,2);
        if (iVar11 == 0) {
          FUN_007ab078(auStack_2a8,"Small Rain Zombie");
          FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2a8);
          FUN_007aa754(auStack_2a8);
          return iVar10;
        }
        FUN_007ab078(auStack_2a8,"Big Rain Zombie");
        FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2a8);
        FUN_007aa754(auStack_2a8);
        return iVar10;
      }
      iVar10 = FUN_007354a8();
      if ((iVar10 == 0) || (iVar10 = FUN_00608330(&DAT_00fd67f4,2), iVar10 != 0)) {
        uVar7 = FUN_00608330(&DAT_00fd67f4,7);
        switch(uVar7) {
        case 0:
          goto switchD_006b3a80_caseD_0;
        case 1:
          iVar10 = FUN_00685550(iVar14,iVar15,0x84,0);
          iVar11 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar11 == 0) {
            FUN_007ab078(auStack_f0,"Small Bald Zombie");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_f0);
            FUN_007aa754(auStack_f0);
            return iVar10;
          }
          if (iVar11 != 1) {
            return iVar10;
          }
          FUN_007ab078(auStack_180,"Big Bald Zombie");
          FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_180);
          FUN_007aa754(auStack_180);
          return iVar10;
        case 2:
          iVar10 = FUN_00685550(iVar14,iVar15,0xba,0);
          iVar11 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar11 == 0) {
            FUN_007ab078(auStack_120,"Small Pincushion Zombie");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_120);
            FUN_007aa754(auStack_120);
            return iVar10;
          }
          if (iVar11 != 1) {
            return iVar10;
          }
          FUN_007ab078(auStack_150,"Big Pincushion Zombie");
          FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_150);
          FUN_007aa754(auStack_150);
          return iVar10;
        case 3:
          iVar10 = FUN_00685550(iVar14,iVar15,0xbb,0);
          iVar11 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar11 == 0) {
            FUN_007ab078(auStack_60,"Small Slimed Zombie");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_60);
            FUN_007aa754(auStack_60);
            return iVar10;
          }
          if (iVar11 != 1) {
            return iVar10;
          }
          FUN_007ab078(auStack_c0,"Big Slimed Zombie");
          FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_c0);
          FUN_007aa754(auStack_c0);
          return iVar10;
        case 4:
          iVar10 = FUN_00685550(iVar14,iVar15,0xbc,0);
          iVar11 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar11 == 0) {
            FUN_007ab078(auStack_1e0,"Small Swamp Zombie");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_1e0);
            FUN_007aa754(auStack_1e0);
            return iVar10;
          }
          if (iVar11 != 1) {
            return iVar10;
          }
          FUN_007ab078(auStack_240,"Big Swamp Zombie");
          FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_240);
          FUN_007aa754(auStack_240);
          return iVar10;
        case 5:
          iVar10 = FUN_00685550(iVar14,iVar15,0xbd,0);
          iVar11 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar11 == 0) {
            FUN_007ab078(auStack_210,"Small Twiggy Zombie");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_210);
            FUN_007aa754(auStack_210);
            return iVar10;
          }
          if (iVar11 != 1) {
            return iVar10;
          }
          FUN_007ab078(auStack_270,"Big Twiggy Zombie");
          FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_270);
          FUN_007aa754(auStack_270);
          return iVar10;
        default:
          iVar10 = FUN_00685550(iVar14,iVar15,200,0);
          iVar11 = FUN_00608330(&DAT_00fd67f4,6);
          if (iVar11 == 0) {
            FUN_007ab078(auStack_2d8,"Small Female Zombie");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
            FUN_007aa754(auStack_2d8);
            return iVar10;
          }
          if (iVar11 != 1) {
            return iVar10;
          }
          FUN_007ab078(auStack_1b0,"Big Female Zombie");
          FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_1b0);
          FUN_007aa754(auStack_1b0);
          return iVar10;
        }
      }
      uVar7 = FUN_005c4e6c(&DAT_00fd67f4,0x13f,0x142);
      uVar12 = 0;
      goto LAB_006b490e;
    }
    if ((cVar13 == '\0') || (iVar10 = FUN_00608330(&DAT_00fd67f4,4), iVar10 != 0)) {
      if ((DAT_01033966 == '\0') || (iVar10 = FUN_00608330(&DAT_00fd67f4,3), iVar10 != 0)) {
        uVar12 = 0;
        uVar7 = 0xa1;
      }
      else {
        uVar12 = 0;
        uVar7 = 0x9b;
      }
      goto LAB_006b490e;
    }
LAB_006b3e62:
    uVar12 = 0;
    uVar7 = 0xa9;
    goto LAB_006b490e;
  }
  if (DAT_01033960 < iVar11) {
    if (local_2e4 + -0xbe < iVar11) {
      uVar7 = 0x3c;
      uVar12 = 0;
      iVar10 = FUN_00608330(&DAT_00fd67f4,0x28);
      if ((iVar10 == 0) && (iVar10 = FUN_00656370(0x27), iVar10 == 0)) {
        uVar7 = 0x27;
        uVar12 = 1;
      }
      else {
        iVar10 = FUN_00608330(&DAT_00fd67f4,0xe);
        if (iVar10 == 0) {
          uVar7 = 0x18;
        }
        else {
          iVar10 = FUN_00608330(&DAT_00fd67f4,8);
          if (iVar10 == 0) {
            iVar10 = FUN_00608330(&DAT_00fd67f4,5);
            if (iVar10 == 0) {
              uVar7 = 0x42;
            }
            else if (iVar10 - 1U < 3) {
              if ((DAT_00fe1f8b == '\0') || (iVar10 = FUN_00608330(&DAT_00fd67f4,5), iVar10 == 0)) {
                uVar7 = 0x3e;
              }
              else {
                uVar7 = 0x9c;
              }
            }
            else {
              uVar7 = 0x3fb;
            }
          }
          else {
            iVar10 = FUN_00608330(&DAT_00fd67f4,3);
            if (iVar10 == 0) {
              uVar7 = 0x3b;
            }
            else if ((DAT_00fe1f8b != '\0') && (iVar10 = FUN_00608330(&DAT_00fd67f4,5), iVar10 != 0)
                    ) {
              uVar7 = 0x97;
            }
          }
        }
      }
      goto LAB_006b490e;
    }
    if ((((iVar16 == 0x93) || (iVar16 == 0xa1)) ||
        ((iVar16 == 0xa2 || ((iVar16 == 0xa3 || (iVar16 == 0xa4)))))) &&
       (((char)param_1[4] == '\0' &&
        ((cVar13 != '\0' && (iVar10 = FUN_00608330(&DAT_00fd67f4,0x1e), iVar10 == 0)))))) {
      iVar10 = (&DAT_0107034c)[param_1[6]];
      if (*(char *)(iVar10 + 0xe6) == '\0') {
        if (*(char *)(iVar10 + 0xe7) == '\0') {
          if (*(char *)(iVar10 + 0xeb) == '\0') {
            return -1;
          }
          uVar12 = 0;
          uVar7 = 0xb4;
        }
        else {
          uVar12 = 0;
          uVar7 = 0xab;
        }
      }
      else {
        uVar12 = 0;
        uVar7 = 0xaa;
      }
      goto LAB_006b490e;
    }
    iVar10 = FUN_00608330(&DAT_00fd67f4,0x3c);
    if (iVar10 == 0) {
      if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) == '\0') {
        uVar12 = 0;
        uVar7 = 0xd9;
      }
      else {
        uVar12 = 0;
        uVar7 = 0xda;
      }
      goto LAB_006b490e;
    }
    if ((char)param_1[4] == '\0') {
      if (cVar13 != '\0') {
        if ((((iVar16 == 0x74) || (iVar16 == 0x75)) || (iVar16 == 0xa4)) &&
           (iVar10 = FUN_00608330(&DAT_00fd67f4,8), iVar10 == 0)) {
          iVar10 = FUN_00608330(&DAT_01070468,4);
          if (iVar10 == 0) {
            uVar7 = 0x3f0;
          }
          else {
            uVar7 = 0x78;
          }
          uVar12 = 0;
          goto LAB_006b490e;
        }
        goto LAB_006b4392;
      }
    }
    else {
LAB_006b4392:
      if (((cVar13 != '\0') && (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) != '\0')) &&
         (iVar10 = FUN_00608330(&DAT_00fd67f4,10), iVar10 == 0)) {
        uVar12 = 0;
        uVar7 = 0x9a;
        goto LAB_006b490e;
      }
    }
    if ((((char)param_1[4] != '\0') || (*(char *)((&DAT_0107034c)[param_1[6]] + 0xe7) != '\0')) ||
       (iVar10 = FUN_00608330(&DAT_00fd67f4,0x4b), iVar10 != 0)) {
      if ((*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) == '\0') ||
         (iVar10 = FUN_00608330(&DAT_00fd67f4,0x14), iVar10 != 0)) {
        if (cVar13 == '\0') {
          iVar10 = FUN_00608330(&DAT_00fd67f4,10);
          if (iVar10 == 0) {
            iVar10 = FUN_00685550(iVar14,iVar15,0x10,0);
            if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) == '\0') {
              return iVar10;
            }
LAB_006b444c:
            FUN_0066c0b8(0xbf800000,iVar10 * 0x274 + DAT_00fe1fa8,0xb8);
            return iVar10;
          }
          iVar10 = FUN_00608330(&DAT_00fd67f4,4);
          if (iVar10 == 0) {
            iVar10 = FUN_00685550(iVar14,iVar15,1,0);
            if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xe9) != '\0') {
              FUN_007ab078(auStack_2d8,"Jungle Slime");
              FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
              FUN_007aa754(auStack_2d8);
              return iVar10;
            }
            if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) == '\0') {
              FUN_007ab078(auStack_2d8,"Black Slime");
              FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
              FUN_007aa754(auStack_2d8);
              return iVar10;
            }
            goto LAB_006b444c;
          }
        }
        iVar10 = FUN_00608330(&DAT_00fd67f4,2);
        if (iVar10 != 0) {
          if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xe9) != '\0') {
            uVar12 = 0;
            uVar7 = 0x33;
            goto LAB_006b490e;
          }
          if ((cVar13 != '\0') && (iVar10 = FUN_00608330(&DAT_00fd67f4,6), 0 < iVar10)) {
            uVar12 = 0;
            uVar7 = 0x5d;
            goto LAB_006b490e;
          }
          if ((iVar16 != 0x93) && ((iVar16 != 0xa1 && (iVar16 != 0xa2)))) {
            uVar12 = 0;
            uVar7 = 0x31;
            goto LAB_006b490e;
          }
          if (cVar13 == '\0') {
            uVar12 = 0;
            uVar7 = 0x96;
            goto LAB_006b490e;
          }
          goto LAB_006b3e62;
        }
        if ((cVar13 != '\0') && (iVar10 = FUN_00608330(&DAT_00fd67f4,10), iVar10 != 0)) {
          iVar10 = FUN_00608330(&DAT_00fd67f4,2);
          if (iVar10 == 0) {
            if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) == '\0') {
              iVar10 = FUN_007354a8();
              if ((iVar10 == 0) || (iVar10 = FUN_00608330(&DAT_00fd67f4,5), iVar10 != 0)) {
                iVar10 = FUN_00685550(iVar14,iVar15,0x4d,0);
                if (iVar11 <= DAT_01033960 + DAT_00902ea4 >> 1) {
                  return iVar10;
                }
                iVar11 = FUN_00608330(&DAT_00fd67f4,5);
                if (iVar11 != 0) {
                  return iVar10;
                }
                FUN_007ab078(auStack_2d8,"Heavy Skeleton");
                FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
                FUN_007aa754(auStack_2d8);
                return iVar10;
              }
              uVar7 = 0x13c;
              uVar12 = 0;
            }
            else {
              uVar12 = 0;
              uVar7 = 0xc5;
            }
          }
          else if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) == '\0') {
            uVar12 = 0;
            uVar7 = 0x6e;
          }
          else {
            uVar12 = 0;
            uVar7 = 0xce;
          }
          goto LAB_006b490e;
        }
        iVar10 = FUN_00608330(&DAT_00fd67f4,0xd);
        if (iVar10 == 0) {
          iVar10 = FUN_00608330(&DAT_01070468,4);
          if (iVar10 == 0) {
            uVar7 = 0x3eb;
          }
          else {
            uVar7 = 0x2c;
          }
          uVar12 = 0;
          goto LAB_006b490e;
        }
        if (((iVar16 == 0x93) || (iVar16 == 0xa1)) || (iVar16 == 0xa2)) {
          uVar12 = 0;
          uVar7 = 0xa7;
          goto LAB_006b490e;
        }
        if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) == '\0') {
          iVar10 = FUN_007354a8();
          if ((iVar10 == 0) || (iVar10 = FUN_00608330(&DAT_00fd67f4,2), iVar10 != 0)) {
            iVar10 = FUN_00608330(&DAT_00fd67f4,4);
            if (iVar10 == 0) {
              iVar10 = FUN_00685550(iVar14,iVar15,0x15,0);
              iVar11 = FUN_00608330(&DAT_00fd67f4,3);
              if (iVar11 != 0) {
                return iVar10;
              }
              iVar11 = FUN_00608330(&DAT_00fd67f4,2);
              if (iVar11 == 0) {
                FUN_007ab078(auStack_2d8,"Big Skeleton");
                FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
                FUN_007aa754(auStack_2d8);
                return iVar10;
              }
              FUN_007ab078(auStack_2d8,"Small Skeleton");
              FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
              FUN_007aa754(auStack_2d8);
              return iVar10;
            }
            if (iVar10 == 1) {
              iVar10 = FUN_00685550(iVar14,iVar15,0xc9,0);
              iVar11 = FUN_00608330(&DAT_00fd67f4,3);
              if (iVar11 != 0) {
                return iVar10;
              }
              iVar11 = FUN_00608330(&DAT_00fd67f4,2);
              if (iVar11 == 0) {
                FUN_007ab078(auStack_2d8,"Big Headache Skeleton");
                FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
                FUN_007aa754(auStack_2d8);
                return iVar10;
              }
              FUN_007ab078(auStack_2d8,"Small Headache Skeleton");
              FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
              FUN_007aa754(auStack_2d8);
              return iVar10;
            }
            if (iVar10 != 2) {
              iVar10 = FUN_00685550(iVar14,iVar15,0xcb,0);
              iVar11 = FUN_00608330(&DAT_00fd67f4,3);
              if (iVar11 != 0) {
                return iVar10;
              }
              iVar11 = FUN_00608330(&DAT_00fd67f4,2);
              if (iVar11 == 0) {
                FUN_007ab078(auStack_2d8,"Big Pantless Skeleton");
                FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
                FUN_007aa754(auStack_2d8);
                return iVar10;
              }
              FUN_007ab078(auStack_2d8,"Small Pantless Skeleton");
              FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
              FUN_007aa754(auStack_2d8);
              return iVar10;
            }
            iVar10 = FUN_00685550(iVar14,iVar15,0xca,0);
            iVar11 = FUN_00608330(&DAT_00fd67f4,3);
            if (iVar11 != 0) {
              return iVar10;
            }
            iVar11 = FUN_00608330(&DAT_00fd67f4,2);
            if (iVar11 == 0) {
              FUN_007ab078(auStack_2d8,"Big Misassembled Skeleton");
              FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
              FUN_007aa754(auStack_2d8);
              return iVar10;
            }
            FUN_007ab078(auStack_2d8,"Small Misassembled Skeleton");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
            FUN_007aa754(auStack_2d8);
            return iVar10;
          }
          uVar7 = FUN_005c4e6c(&DAT_00fd67f4,0x142,0x145);
          uVar12 = 0;
          goto LAB_006b490e;
        }
      }
      uVar12 = 0;
      uVar7 = 0xb9;
      goto LAB_006b490e;
    }
    if (cVar13 == '\0') goto LAB_006b4110;
  }
  else {
    if (((char)param_1[4] != '\0') || (iVar10 = FUN_00608330(&DAT_00fd67f4,0x32), iVar10 != 0)) {
      if ((DAT_01033966 == '\0') || (iVar10 = FUN_00608330(&DAT_00fd67f4,3), iVar10 != 0)) {
        if ((cVar13 == '\0') || (iVar10 = FUN_00608330(&DAT_00fd67f4,4), iVar10 == 0)) {
          if ((iVar16 != 0x93) &&
             ((iVar16 != 0xa1 && (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) == '\0')))) {
            iVar10 = FUN_00685550(iVar14,iVar15,1,0);
            iVar11 = FUN_00608330(&DAT_00fd67f4,5);
            if (iVar11 == 0) {
              FUN_007ab078(auStack_2d8,"Yellow Slime");
              FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
              FUN_007aa754(auStack_2d8);
              return iVar10;
            }
            iVar11 = FUN_00608330(&DAT_00fd67f4,2);
            if (iVar11 != 0) {
              return iVar10;
            }
            FUN_007ab078(auStack_2d8,"Red Slime");
            FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
            FUN_007aa754(auStack_2d8);
            return iVar10;
          }
          uVar12 = 0;
          uVar7 = 0x93;
        }
        else {
          uVar12 = 0;
          uVar7 = 0x8d;
        }
        goto LAB_006b490e;
      }
LAB_006b3970:
      uVar12 = 0;
      uVar7 = 0x8c;
      goto LAB_006b490e;
    }
    if ((cVar13 == '\0') || (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) != '\0')) {
LAB_006b4110:
      if (*(char *)((&DAT_0107034c)[param_1[6]] + 0xea) == '\0') {
        uVar12 = 1;
        uVar7 = 10;
      }
      else {
        uVar12 = 0;
        uVar7 = 0xb9;
      }
      goto LAB_006b490e;
    }
  }
  uVar12 = 1;
  uVar7 = 0x5f;
LAB_006b490e:
  iVar10 = FUN_00685550(iVar14,iVar15,uVar7,uVar12);
  return iVar10;
switchD_006b3ed8_caseD_0:
  iVar10 = FUN_00685550(iVar14,iVar15,0xbe,0);
  iVar11 = FUN_00608330(&DAT_00fd67f4,3);
  if (iVar11 != 0) {
    return iVar10;
  }
  FUN_007ab078(auStack_2d8,"Cataract Eye 2");
  FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2d8);
  FUN_007aa754(auStack_2d8);
  return iVar10;
switchD_006b3a80_caseD_0:
  iVar10 = FUN_00685550(iVar14,iVar15,3,0);
  iVar11 = FUN_00608330(&DAT_00fd67f4,6);
  if (iVar11 == 0) {
    FUN_007ab078(auStack_90,"Small Zombie");
    FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_90);
    FUN_007aa754(auStack_90);
    return iVar10;
  }
  if (iVar11 != 1) {
    return iVar10;
  }
  FUN_007ab078(auStack_2a8,"Big Zombie");
  FUN_0067a84c(iVar10 * 0x274 + DAT_00fe1fa8,auStack_2a8);
  FUN_007aa754(auStack_2a8);
  return iVar10;
}


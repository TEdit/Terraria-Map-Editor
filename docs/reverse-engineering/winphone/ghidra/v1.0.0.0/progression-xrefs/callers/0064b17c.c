
void FUN_0064b17c(void)

{
  undefined1 *puVar1;
  byte bVar2;
  bool bVar3;
  undefined1 uVar4;
  undefined2 uVar5;
  short sVar6;
  uint uVar7;
  int iVar8;
  uint uVar9;
  undefined4 *puVar10;
  undefined1 *puVar11;
  undefined4 uVar12;
  int *piVar13;
  undefined4 uVar14;
  undefined4 uVar15;
  int iVar16;
  undefined4 uVar17;
  int iVar18;
  void *_Dst;
  undefined1 *puVar19;
  uint uVar20;
  undefined2 *puVar21;
  int iVar22;
  undefined4 *puVar23;
  undefined1 *puVar24;
  undefined1 uVar25;
  uint *puVar26;
  undefined *puVar27;
  undefined4 *puVar28;
  undefined1 *puVar29;
  undefined1 *puVar30;
  ushort *puVar31;
  uint uVar32;
  undefined2 *puVar33;
  uint in_fpscr;
  float fVar34;
  float fVar35;
  undefined8 uVar36;
  undefined8 uVar37;
  int local_65c;
  undefined1 *local_658;
  uint local_654;
  undefined4 local_64c;
  undefined4 local_648;
  undefined1 auStack_644 [8];
  undefined1 auStack_63c [8];
  undefined1 auStack_634 [8];
  undefined2 local_62c [8];
  undefined4 local_61c;
  undefined4 local_618;
  undefined2 local_614 [8];
  undefined4 local_604;
  undefined4 local_600;
  undefined2 local_5fc [8];
  undefined4 local_5ec;
  undefined4 local_5e8;
  undefined1 auStack_5e4 [24];
  undefined1 auStack_5cc [24];
  undefined auStack_5b4 [24];
  undefined1 auStack_59c [8];
  undefined1 auStack_594 [8];
  undefined4 local_58c;
  undefined4 uStack_588;
  undefined1 auStack_584 [8];
  undefined1 auStack_57c [24];
  undefined1 auStack_564 [8];
  undefined1 auStack_55c [24];
  undefined1 auStack_544 [8];
  undefined1 auStack_53c [8];
  undefined1 auStack_534 [8];
  undefined1 auStack_52c [8];
  undefined1 auStack_524 [24];
  undefined1 auStack_50c [24];
  undefined1 auStack_4f4 [48];
  undefined1 auStack_4c4 [48];
  undefined1 auStack_494 [24];
  undefined1 auStack_47c [48];
  undefined1 auStack_44c [24];
  undefined1 auStack_434 [48];
  undefined1 auStack_404 [24];
  undefined1 auStack_3ec [48];
  undefined1 auStack_3bc [48];
  undefined1 auStack_38c [48];
  undefined1 auStack_35c [48];
  undefined1 auStack_32c [48];
  undefined1 auStack_2fc [48];
  undefined1 auStack_2cc [48];
  undefined1 auStack_29c [48];
  undefined1 auStack_26c [48];
  undefined1 auStack_23c [48];
  undefined1 auStack_20c [48];
  undefined1 auStack_1dc [48];
  undefined1 auStack_1ac [48];
  undefined1 auStack_17c [48];
  undefined1 *local_14c;
  short local_148 [2];
  undefined1 *local_144;
  undefined4 local_140;
  undefined *local_13c;
  undefined4 local_134;
  undefined4 local_130;
  undefined1 auStack_12c [48];
  undefined1 auStack_fc [48];
  undefined1 auStack_cc [4];
  undefined4 local_c8;
  undefined2 local_78;
  undefined2 local_76;
  
  uVar7 = FUN_007ffb80();
  local_58c = 0xfffffffe;
  uStack_588 = 0xfffffffe;
  local_654 = uVar7;
  (**(code **)(DAT_00fe1dd8 + 0x1c))(&DAT_00fe1dd8,local_148,1);
  puVar19 = DAT_0064b330;
  bVar2 = (byte)local_148[0];
  if (DAT_00fe1ec4 != 1) {
    if (DAT_00fe1ec4 == 2) {
      if ((byte)local_148[0] < 0x40) {
        if ((byte)local_148[0] == 0x3f) {
          FUN_0068573c();
        }
        else if ((byte)local_148[0] < 0x20) {
          if ((byte)local_148[0] == 0x1f) {
            local_13c = (undefined *)FUN_00644250(&DAT_00fe1dd8);
            local_658 = (undefined1 *)(&DAT_0107034c)[(int)local_13c];
            uVar14 = FUN_00644314(&DAT_00fe1dd8);
            uVar15 = FUN_00644314(&DAT_00fe1dd8);
            iVar8 = FUN_006056e0(uVar14,uVar15);
            if (iVar8 != -1) {
              iVar16 = FUN_00605a10();
              local_14c = (undefined1 *)FUN_00605844(uVar14,uVar15);
              if (iVar16 == 0) {
                if (((local_14c == (undefined1 *)0x0) && (iVar8 != -2)) && (iVar8 != -3)) {
                  iVar16 = 0;
                  local_144 = &DAT_00fe1e78;
                  do {
                    EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                    DAT_00fe1e90 = 1;
                    FUN_00646500(0x20,iVar8,iVar16);
                    LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                    DAT_00fe1e90 = 0;
                    FUN_006448b4(uVar7);
                    iVar16 = iVar16 + 1;
                  } while (iVar16 < 0x28);
                }
                if (local_14c == (undefined1 *)0x0) {
                  local_144 = &DAT_00fe1e78;
                  EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                  DAT_00fe1e90 = 1;
                  FUN_00646500(0x21,local_13c,iVar8);
                  LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                  DAT_00fe1e90 = 0;
                  FUN_006448b4(uVar7);
                  *(short *)(local_658 + 0x8de2) = (short)iVar8;
                }
              }
            }
          }
          else {
            switch((byte)local_148[0]) {
            case 1:
              iVar8 = FUN_00644250(&DAT_00fe1dd8);
              if (*(short *)(uVar7 + 0x10) == 0) {
                if (iVar8 != 6) {
                  uVar14 = FUN_00644250(&DAT_00fe1dd8);
                  FUN_00649fe8(uVar14,0x16);
                  break;
                }
                *(undefined2 *)(uVar7 + 0x10) = 1;
              }
              uVar14 = FUN_00644250(&DAT_00fe1dd8);
              FUN_006485c4(uVar7,uVar14);
              break;
            default:
              goto switchD_0064c074_caseD_2;
            case 6:
              if (*(short *)(uVar7 + 0x10) == 1) {
                *(undefined2 *)(uVar7 + 0x10) = 2;
              }
              FUN_00647dc0(uVar7);
              iVar8 = 0;
              if (0 < DAT_00fe1fac) {
                iVar18 = 0;
                iVar16 = DAT_00fe1fac;
                do {
                  if (*(char *)(DAT_00fe1fa8 + iVar18 + 0x100) != '\0') {
                    FUN_006483ac(iVar8,uVar7);
                    iVar16 = DAT_00fe1fac;
                  }
                  iVar8 = iVar8 + 1;
                  iVar18 = iVar18 + 0x274;
                } while (iVar8 < iVar16);
              }
              break;
            case 8:
              local_14c = (undefined1 *)FUN_00644250(&DAT_00fe1dd8);
              iVar8 = FUN_006442ec(&DAT_00fe1dd8);
              iVar16 = FUN_006442ec(&DAT_00fe1dd8);
              if ((iVar8 < 0) || (iVar16 < 0)) {
                bVar3 = false;
              }
              else {
                bVar3 = true;
              }
              if ((bVar3) &&
                 (((iVar8 < 10 || (DAT_00902ea0 + -10 < iVar8)) ||
                  ((iVar16 < 10 || (DAT_00902ea4 + -10 < iVar16)))))) {
                bVar3 = false;
              }
              uVar14 = 0x24;
              if (bVar3) {
                uVar14 = 0x48;
              }
              if (*(short *)(uVar7 + 0x10) == 2) {
                *(undefined2 *)(uVar7 + 0x10) = 3;
              }
              FUN_006484b8(uVar7,uVar14);
              iVar18 = (int)((ulonglong)((longlong)(int)DAT_010338cc * (longlong)DAT_0064c768) >>
                            0x20);
              iVar22 = (int)((ulonglong)((longlong)(int)DAT_010338c8 * (longlong)DAT_0064c764) >>
                            0x20) + (int)DAT_010338c8;
              FUN_0064a14c(uVar7,(iVar18 >> 3) - (iVar18 >> 0x1f),(iVar22 >> 3) - (iVar22 >> 0x1f),6
                          );
              if (bVar3) {
                iVar8 = (int)((ulonglong)((longlong)iVar8 * (longlong)DAT_0064c768) >> 0x20);
                iVar16 = (int)((ulonglong)((longlong)iVar16 * (longlong)DAT_0064c764) >> 0x20) +
                         iVar16;
                FUN_0064a14c(uVar7,(iVar8 >> 3) - (iVar8 >> 0x1f),(iVar16 >> 3) - (iVar16 >> 0x1f),6
                            );
              }
              puVar27 = &DAT_010731a0;
              local_658 = (undefined1 *)0x0;
              do {
                if (puVar27[8] != '\0') {
                  iVar8 = FUN_0058ef48();
                  uVar4 = *(undefined1 *)(iVar8 + 0x20);
                  local_144 = &DAT_00fe1e78;
                  EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                  DAT_00fe1e90 = 1;
                  FUN_00646500(0x15,uVar4,local_658);
                  LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                  DAT_00fe1e90 = 0;
                  FUN_006448b4(uVar7);
                  local_13c = &DAT_00fe1e78;
                  EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                  DAT_00fe1e90 = 1;
                  FUN_00645808(0x16,local_658);
                  LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                  DAT_00fe1e90 = 0;
                  FUN_006448b4(uVar7);
                }
                local_658 = local_658 + 1;
                puVar27 = puVar27 + 0xa0;
              } while ((int)local_658 < 200);
              local_65c = 0;
              iVar8 = 0;
              do {
                iVar16 = iVar8 + DAT_00fe1fa8;
                if (*(char *)(iVar16 + 0x100) != '\0') {
                  if (*(char *)(iVar16 + 0x118) != '\0') {
                    iVar18 = (int)((ulonglong)
                                   ((longlong)*(int *)(iVar16 + 0x148) * (longlong)DAT_0064c768) >>
                                  0x20);
                    iVar16 = *(int *)(iVar16 + 0x14c) +
                             (int)((ulonglong)
                                   ((longlong)*(int *)(iVar16 + 0x14c) * (longlong)DAT_0064c764) >>
                                  0x20);
                    FUN_0064a14c(uVar7,(iVar18 >> 7) - (iVar18 >> 0x1f),
                                 (iVar16 >> 7) - (iVar16 >> 0x1f),6);
                  }
                  local_144 = &DAT_00fe1e78;
                  EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                  DAT_00fe1e90 = 1;
                  FUN_00645808(0x17,local_65c);
                  LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
                  DAT_00fe1e90 = 0;
                  FUN_006448b4(uVar7);
                }
                local_65c = local_65c + 1;
                iVar8 = iVar8 + 0x274;
              } while (iVar8 < DAT_0064c760);
              puVar21 = &DAT_0107af40;
              iVar8 = 0;
              do {
                if ((*(char *)(puVar21 + 2) != '\0') &&
                   ((*(char *)(puVar21 + 8) != '\0' ||
                    (iVar16 = FUN_006444b0(*puVar21), iVar16 != 0)))) {
                  FUN_006473d8(iVar8,2);
                }
                iVar8 = iVar8 + 1;
                puVar21 = puVar21 + 0x74;
              } while (iVar8 < 0x200);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x11);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x12);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x13);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x14);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x16);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x26);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x36);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x6b);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x6c);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0x7c);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0xa0);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0xb2);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0xcf);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0xd1);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0xd0);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0xe3);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0xe4);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00645808(0x39,0xe5);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_006452c4(0x3a);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
              FUN_00648244(local_14c,uVar7);
              break;
            case 0xb:
              FUN_00647d24(uVar7);
            }
          }
        }
        else if ((byte)local_148[0] < 0x37) {
          if ((byte)local_148[0] == 0x36) {
            iVar8 = FUN_00644250(&DAT_00fe1dd8);
            uVar14 = FUN_00644250(&DAT_00fe1dd8);
            uVar15 = FUN_006448f8();
            FUN_00659cd4(iVar8 * 0x274 + DAT_00fe1fa8,uVar14,uVar15,1);
            FUN_006481dc(iVar8);
          }
          else if ((byte)local_148[0] == 0x22) {
            iVar8 = FUN_00644314(&DAT_00fe1dd8);
            iVar16 = FUN_00644314(&DAT_00fe1dd8);
            if ((*(short *)((iVar8 * DAT_00902934 + iVar16) * 0xe + DAT_00902928 + 6) == 0x15) &&
               (iVar18 = FUN_00761b80(iVar8,iVar16), iVar18 != 0)) {
              FUN_00647acc(0,iVar8,iVar16,0,0);
            }
          }
          else {
            if ((byte)local_148[0] != 0x2f) goto switchD_0064c074_caseD_2;
            uVar14 = FUN_00644250(&DAT_00fe1dd8);
            uVar15 = FUN_00644314(&DAT_00fe1dd8);
            uVar17 = FUN_00644314(&DAT_00fe1dd8);
            iVar8 = FUN_007261f8(uVar15,uVar17);
            if (-1 < iVar8) {
              local_144 = &DAT_00fe1e78;
              EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 1;
              FUN_00646500(0x30,uVar14,iVar8);
              LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e78);
              DAT_00fe1e90 = 0;
              FUN_006448b4(uVar7);
            }
          }
        }
        else {
          if ((byte)local_148[0] != 0x3e) goto switchD_0064c074_caseD_2;
          iVar8 = FUN_00644250(&DAT_00fe1dd8);
          iVar16 = FUN_006442ec(&DAT_00fe1dd8);
          if (iVar16 == 0x71) {
            local_144 = *(undefined1 **)((&DAT_0107034c)[iVar8] + 0x110);
            local_140 = *(undefined4 *)((&DAT_0107034c)[iVar8] + 0x114);
            FUN_006b77dc(&local_144);
          }
          else if (iVar16 < 0) {
            if (iVar16 == -4) {
              if (DAT_010703e0 == '\0') {
                FUN_007350a0();
                FUN_00647dc0(0);
                FUN_00646f20(0x31,0x32,0xff,0x82,0xffffffff);
                goto LAB_0064cc82;
              }
            }
            else if (DAT_010339a0 == 0) {
              uVar14 = 0;
              DAT_010339ac = 0;
              if (iVar16 == -1) {
                uVar14 = 1;
              }
              else if (iVar16 == -2) {
                uVar14 = 2;
              }
              else if (iVar16 == -3) {
                uVar14 = 3;
              }
              FUN_007a8f74(uVar14);
            }
          }
          else {
            bVar3 = true;
            iVar22 = 0;
            iVar18 = DAT_00fe1fa8;
            do {
              if ((*(int *)(iVar18 + 0x104) == iVar16) && (*(char *)(iVar18 + 0x100) != '\0')) {
                bVar3 = false;
                break;
              }
              iVar22 = iVar22 + 1;
              iVar18 = iVar18 + 0x274;
            } while (iVar22 < 0xc4);
            if (bVar3) {
              FUN_00685904((&DAT_0107034c)[iVar8],iVar16);
            }
          }
        }
      }
      else {
        switch((byte)local_148[0]) {
        case 0x43:
          FUN_00643a94(uVar7);
          break;
        case 0x44:
          FUN_00643a7c(uVar7);
          break;
        default:
          goto switchD_0064c074_caseD_2;
        case 0x4c:
          iVar8 = FUN_0064f3e8();
          iVar16 = *(int *)(uVar7 + 0xc);
          if (iVar8 != -1) {
            local_14c = (undefined1 *)(&DAT_0107034c)[iVar8];
            local_14c[0x5cc9] = (char)iVar8;
            *(undefined4 *)(local_14c + 0x30) = *(undefined4 *)(**(int **)(iVar16 + 0xc) + 0x30);
            FUN_0064525c(iVar16 + 0xc,*(undefined4 *)(iVar16 + 0x14),&local_14c);
          }
          FUN_006486b8(iVar16,iVar8);
          break;
        case 0x4d:
          uVar14 = FUN_00644250(&DAT_00fe1dd8);
          FUN_006442ec(&DAT_00fe1dd8);
          FUN_006442ec(&DAT_00fe1dd8);
          FUN_00648244(uVar14,uVar7);
          break;
        case 0x4e:
          puVar27 = *(undefined **)(uVar7 + 0xc);
          local_13c = puVar27;
          local_14c = (undefined1 *)FUN_00644250(&DAT_00fe1dd8);
          iVar8 = 0;
          if (0 < *(int *)(puVar27 + 0x14)) {
            do {
              iVar16 = *(int *)(*(int *)(puVar27 + 0xc) + iVar8 * 4);
              if ((undefined1 *)(uint)*(byte *)(iVar16 + 0x5cc9) == local_14c) {
                FUN_006507f0(iVar16);
                *(undefined1 *)(iVar16 + 0x5cc9) = 0xff;
                *(undefined4 *)(iVar16 + 0x30) = 0;
                *(undefined1 *)((&DAT_0107034c)[(int)local_14c] + 0x5cc5) = 0;
                iVar16 = *(int *)(local_13c + 0x14) + -1;
                *(int *)(local_13c + 0x14) = iVar16;
                if (iVar8 < iVar16) {
                  _Dst = (void *)(*(int *)(local_13c + 0xc) + iVar8 * 4);
                  memmove(_Dst,(void *)((int)_Dst + 4),(iVar16 - iVar8) * 4);
                }
                break;
              }
              iVar8 = iVar8 + 1;
            } while (iVar8 < *(int *)(puVar27 + 0x14));
          }
          break;
        case 0x4f:
          iVar8 = FUN_00644314(&DAT_00fe1dd8);
          iVar16 = FUN_00644314(&DAT_00fe1dd8);
          uVar14 = FUN_00644228(&DAT_00fe1dd8);
          if ((*(short *)((iVar8 * DAT_00902934 + iVar16) * 0xe + DAT_00902928 + 6) == 10) &&
             (iVar18 = FUN_00776dc8(iVar8,iVar16,uVar14), iVar18 != 0)) {
            FUN_00648da4(iVar8,iVar16,uVar14);
          }
          break;
        case 0x50:
          iVar8 = FUN_00644314(&DAT_00fe1dd8);
          iVar16 = FUN_00644314(&DAT_00fe1dd8);
          if ((*(short *)((iVar8 * DAT_00902934 + iVar16) * 0xe + DAT_00902928 + 6) == 0xb) &&
             (iVar18 = FUN_0075784c(iVar8,iVar16), iVar18 != 0)) {
            FUN_00772624(iVar8,iVar16,0);
            FUN_006494f4(iVar8,iVar16);
          }
          break;
        case 0x57:
          uVar14 = FUN_00644250(&DAT_00fe1dd8);
          uVar15 = FUN_00644250(&DAT_00fe1dd8);
          local_144 = (undefined1 *)VectorSignedToFloat(uVar14,(byte)(in_fpscr >> 0x16) & 3);
          local_140 = VectorSignedToFloat(uVar15,(byte)(in_fpscr >> 0x16) & 3);
          FUN_00697cec(local_144,local_140);
        }
      }
      goto switchD_0064b212_caseD_1;
    }
    goto switchD_0064c074_caseD_2;
  }
  iVar8 = FUN_0058ef48();
  if (0 < DAT_00fe1eac) {
    DAT_00fe1eb0 = DAT_00fe1eb0 + 1;
  }
  switch(bVar2) {
  case 0:
    puVar11 = (undefined1 *)*DAT_00902020;
    local_658 = puVar11;
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    if (iVar8 == 0xff) {
      DAT_010339bc = 0;
      FUN_0074134c(puVar11);
    }
    else {
      FUN_0073ae14(puVar11,iVar8);
      **(undefined4 **)(*(int *)(puVar11 + 0x10) + 0xc) = (&DAT_0107034c)[iVar8];
      FUN_00647f6c();
      DAT_00902028 = DAT_00902028 + -1;
      if (0 < DAT_00902028) {
        memmove(DAT_00902020,DAT_00902020 + 1,DAT_00902028 * 4);
      }
      FUN_006451f4(&DAT_0090202c,DAT_00902034,&local_658);
    }
    break;
  case 2:
    DAT_00fe1ebd = 1;
    iVar16 = FUN_00644314(&DAT_00fe1dd8);
    FUN_007aa80c(iVar8 + 0x2b10,auStack_32c,&DAT_00fd6948 + iVar16 * 0x30);
    FUN_007aa754(auStack_32c);
    break;
  case 3:
    if (DAT_00fe1eb4 == 3) {
      DAT_00fe1eb4 = 4;
    }
    uVar14 = FUN_00644250(&DAT_00fe1dd8);
    FUN_00648550(uVar14,0,0);
    FUN_00649f70(uVar14,0,0);
    FUN_00649bb0(uVar14,0);
    FUN_00649ca0(uVar14,0);
    iVar8 = 0;
    do {
      FUN_00649560(uVar14,iVar8);
      iVar8 = iVar8 + 1;
    } while (iVar8 < 0x31);
    iVar8 = 0;
    do {
      FUN_00649560(uVar14,iVar8 + 0x31);
      iVar8 = iVar8 + 1;
    } while (iVar8 < 0xb);
    FUN_00647ec0();
    break;
  case 7:
    DAT_010703dc = FUN_0064433c(&DAT_00fe1dd8);
    uVar7 = FUN_00644250(&DAT_00fe1dd8);
    DAT_010703e0 = (uVar7 & 1) != 0;
    DAT_010703e1 = (uVar7 & 2) != 0;
    DAT_010703e2 = (uVar7 & 4) != 0;
    DAT_010703e3 = (uVar7 & 8) != 0;
    DAT_010703e4 = (undefined1)((int)uVar7 >> 5);
    DAT_00902ea0 = FUN_006442ec(&DAT_00fe1dd8);
    DAT_00902ea4 = FUN_006442ec(&DAT_00fe1dd8);
    DAT_010338cc = FUN_006442ec(&DAT_00fe1dd8);
    DAT_010338c8 = FUN_006442ec(&DAT_00fe1dd8);
    DAT_00902ebc = FUN_006442ec(&DAT_00fe1dd8);
    DAT_01050304 = DAT_00902ebc << 4;
    DAT_01033960 = FUN_006442ec(&DAT_00fe1dd8);
    DAT_0103395c = DAT_01033960 << 4;
    iVar8 = FUN_00644278(&DAT_00fe1dd8);
    if (iVar8 != DAT_01033974) {
      DAT_01033967 = 1;
      DAT_01033974 = iVar8;
    }
    iVar8 = FUN_00644278(&DAT_00fe1dd8);
    if (iVar8 != DAT_01033970) {
      DAT_01033967 = 1;
      DAT_01033970 = iVar8;
    }
    FUN_006449f8(&DAT_0102a710,&DAT_00fe1dd8);
    FUN_00644978(&DAT_0102a704,&DAT_00fe1dd8);
    uVar7 = FUN_00644250(&DAT_00fe1dd8);
    DAT_0102a700 = uVar7 & 3;
    DAT_0102a6f8 = (int)uVar7 >> 2 & 3;
    DAT_0102a6fc = (int)uVar7 >> 4;
    DAT_0102a71c = FUN_00644250(&DAT_00fe1dd8);
    DAT_0102a720 = FUN_00644250(&DAT_00fe1dd8);
    DAT_0102a724 = FUN_00644250(&DAT_00fe1dd8);
    DAT_0102a728 = FUN_00644250(&DAT_00fe1dd8);
    DAT_0102a72c = FUN_00644250(&DAT_00fe1dd8);
    DAT_0102a730 = FUN_00644250(&DAT_00fe1dd8);
    DAT_0102a734 = FUN_00644250(&DAT_00fe1dd8);
    DAT_0102a738 = FUN_00644250(&DAT_00fe1dd8);
    FUN_005ff46c(0,DAT_0102a71c);
    FUN_005ff46c(1,DAT_0102a720);
    FUN_005ff46c(2,DAT_0102a724);
    FUN_005ff46c(3,DAT_0102a728);
    FUN_005ff46c(4,DAT_0102a72c);
    FUN_005ff46c(5,DAT_0102a730);
    FUN_005ff46c(6,DAT_0102a734);
    FUN_005ff46c(7,DAT_0102a738);
    DAT_00901160 = FUN_00644250(&DAT_00fe1dd8);
    DAT_01029998 = FUN_0064433c(&DAT_00fe1dd8);
    DAT_010299a8 = (float)FUN_0064433c(&DAT_00fe1dd8);
    uVar7 = in_fpscr & 0xfffffff | (uint)(DAT_010299a8 < 0.0) << 0x1f |
            (uint)(DAT_010299a8 == 0.0) << 0x1e;
    in_fpscr = uVar7 | (uint)NAN(DAT_010299a8) << 0x1c;
    bVar2 = (byte)(uVar7 >> 0x18);
    DAT_010299a0 = !(bool)(bVar2 >> 6 & 1) && bVar2 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1);
    uVar7 = FUN_00644314(&DAT_00fe1dd8);
    DAT_0102a76f = (byte)uVar7 & 1;
    DAT_00fe1f73 = (byte)((int)uVar7 >> 1) & 1;
    DAT_00fe1f7a = (byte)((int)uVar7 >> 2) & 1;
    DAT_00fe1f7b = (byte)((int)uVar7 >> 3) & 1;
    DAT_010338d3 = (byte)((int)uVar7 >> 4) & 1;
    DAT_00fe1f85 = (byte)((int)uVar7 >> 5) & 1;
    DAT_00fe1f8d = (byte)((int)uVar7 >> 6) & 1;
    DAT_00fe1f88 = (byte)((int)uVar7 >> 7) & 1;
    DAT_00fe1f89 = (byte)(uVar7 >> 8) & 1;
    DAT_00fe1f8a = (byte)((int)uVar7 >> 9) & 1;
    DAT_00fe1f8b = (uVar7 & 0x380) != 0;
    DAT_0102a787 = (byte)((int)uVar7 >> 10) & 1;
    DAT_010299b4 = VectorSignedToFloat((uint)((uVar7 & 0x800) != 0),(byte)(in_fpscr >> 0x16) & 3);
    FUN_00734180(&DAT_010703d8,(int)uVar7 >> 0xc);
    uVar14 = FUN_0064a1d4(&DAT_00fe1dd8,auStack_2cc);
    FUN_007aa80c(&DAT_01070308,auStack_23c,uVar14);
    FUN_007aa754(auStack_23c);
    FUN_007aa754(auStack_2cc);
    FUN_0074a4ac();
    FUN_0074a79c();
    uVar7 = local_654;
    if (DAT_00fe1eb4 < 6) {
      DAT_00fe1eb4 = 6;
      FUN_0041c4d8(auStack_57c,"TXT_REQUESTING_TILE_DATA");
      uVar14 = FUN_005f82b0(auStack_3bc,auStack_57c);
      uVar15 = FUN_0058ef48();
      FUN_00736950(uVar15,uVar14);
      FUN_007aa754(auStack_3bc);
      FUN_0041b8a0(auStack_57c);
      uVar7 = local_654;
    }
    break;
  case 9:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    DAT_00fe1eac = DAT_00fe1eac + iVar8;
    break;
  case 10:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    puVar27 = (undefined *)(iVar8 * 0x14);
    local_13c = puVar27;
    iVar16 = FUN_00644250(&DAT_00fe1dd8);
    local_14c = (undefined1 *)(iVar16 * 0xf);
    iVar16 = 0;
    puVar10 = (undefined4 *)0x0;
    if ((int)puVar27 < (int)(puVar27 + 0x14)) {
      puVar23 = (undefined4 *)((int)(local_14c + iVar8 * 20000) * 0xe + DAT_00902928);
      local_658 = puVar27 + 0x14 + iVar8 * -0x14;
      do {
        iVar8 = 0xe;
        puVar28 = puVar23;
        do {
          if (iVar16 == 0) {
            FUN_00730d74(puVar28,&DAT_00fe1dd8);
            iVar16 = FUN_006448f8();
            puVar10 = puVar28;
          }
          else {
            iVar16 = iVar16 + -1;
            *puVar28 = *puVar10;
            puVar28[1] = puVar10[1];
            puVar28[2] = puVar10[2];
            *(undefined2 *)(puVar28 + 3) = *(undefined2 *)(puVar10 + 3);
          }
          iVar8 = iVar8 + -1;
          puVar28 = (undefined4 *)((int)puVar28 + 0xe);
        } while (-1 < iVar8);
        puVar23 = puVar23 + 0xdac;
        local_658 = local_658 + -1;
      } while (local_658 != (undefined *)0x0);
      local_658 = (undefined1 *)0x0;
      puVar27 = local_13c;
    }
    FUN_00772914(puVar27,local_14c);
    break;
  case 0xe:
    uVar9 = FUN_00644250(&DAT_00fe1dd8);
    uVar32 = uVar9 & 0x80 ^ uVar9;
    iVar8 = (&DAT_0107034c)[uVar32];
    if ((uVar9 & 0x80) == 0) {
      *(undefined1 *)(iVar8 + 0x5cc5) = 0;
    }
    else {
      if (*(char *)(iVar8 + 0x5cc5) == '\0') {
        FUN_006d62b4(iVar8);
        *(undefined1 *)(iVar8 + 0x5cc5) = 1;
      }
      FUN_0064f4e0(uVar32);
    }
    break;
  case 0x12:
  case 0x19:
  case 0x25:
  case 0x26:
  case 0x27:
  case 0x40:
    uVar14 = FUN_00644250(&DAT_00fe1dd8);
    local_13c = (undefined *)FUN_00644250(&DAT_00fe1dd8);
    local_14c = (undefined1 *)FUN_00644250(&DAT_00fe1dd8);
    iVar8 = 0;
    local_140 = 0;
    FUN_007aa9a8(auStack_12c);
    if (bVar2 == 0x12) {
      iVar8 = FUN_00644314(&DAT_00fe1dd8);
      local_140 = 0;
      FUN_007aa80c(auStack_12c,auStack_26c,&DAT_00fd6948 + iVar8 * 0x30);
      puVar11 = auStack_26c;
LAB_0064be72:
      FUN_007aa754(puVar11);
    }
    else {
      if (bVar2 == 0x27) {
        iVar8 = FUN_00644250(&DAT_00fe1dd8);
        local_140 = 0;
        uVar15 = FUN_006442ec(&DAT_00fe1dd8);
        uVar17 = FUN_007aa6e0(&DAT_00fd6948 + iVar8 * 0x30);
        uVar15 = FUN_007ab778(auStack_17c,uVar17,uVar15);
        FUN_007aa80c(auStack_12c,auStack_1dc,uVar15);
        FUN_007aa754(auStack_1dc);
        puVar11 = auStack_17c;
        goto LAB_0064be72;
      }
      if (bVar2 == 0x40) {
        uVar36 = FUN_0064429c(&DAT_00fe1dd8);
        FUN_0064a1d4(&DAT_00fe1dd8,auStack_fc);
        uVar37 = FUN_00595220(auStack_20c,auStack_fc);
        uVar15 = FUN_00637788(auStack_38c,(int)((ulonglong)uVar37 >> 0x20),(int)uVar36,
                              (int)((ulonglong)uVar36 >> 0x20),(int)uVar37);
        FUN_007aa80c(auStack_12c,auStack_1ac,uVar15);
        FUN_007aa754(auStack_1ac);
        FUN_007aa754(auStack_38c);
LAB_0064be6e:
        puVar11 = auStack_fc;
        goto LAB_0064be72;
      }
      if (bVar2 != 0x19) {
        iVar8 = FUN_00644250(&DAT_00fe1dd8);
        local_140 = 0;
      }
      uVar15 = FUN_0064a1d4(&DAT_00fe1dd8,auStack_2fc);
      FUN_007aa80c(auStack_12c,auStack_35c,uVar15);
      FUN_007aa754(auStack_35c);
      FUN_007aa754(auStack_2fc);
      if ((bVar2 == 0x25) || (bVar2 == 0x26)) {
        FUN_00595220(auStack_fc,auStack_12c);
        FUN_007aa80c(auStack_12c,auStack_29c,&DAT_00fd6948 + iVar8 * 0x30);
        FUN_007aa754(auStack_29c);
        local_658 = auStack_50c;
        uVar15 = FUN_007aa7e4(auStack_fc,auStack_50c);
        local_144 = auStack_44c;
        uVar17 = FUN_00426e64(auStack_44c,L"#PlayerName#");
        uVar12 = FUN_007aa5e0(auStack_12c);
        local_600 = 7;
        local_604 = 0;
        local_614[0] = 0;
        FUN_00425168(local_614,uVar12,0,0xffffffff);
        piVar13 = (int *)FUN_00426810(auStack_524,local_614,uVar17,uVar15);
        if (7 < (uint)piVar13[5]) {
          piVar13 = (int *)*piVar13;
        }
        FUN_007aaab8(auStack_12c,auStack_4c4,piVar13);
        FUN_007aa754(auStack_4c4);
        FUN_00423f74(auStack_524);
        local_144 = auStack_494;
        uVar15 = FUN_007aa7e4(auStack_fc,auStack_494);
        local_658 = auStack_404;
        uVar17 = FUN_00426e64(auStack_404,L"#BossName#");
        uVar12 = FUN_007aa5e0(auStack_12c);
        local_5e8 = 7;
        local_5ec = 0;
        local_5fc[0] = 0;
        FUN_00425168(local_5fc,uVar12,0,0xffffffff);
        piVar13 = (int *)FUN_00426810(auStack_55c,local_5fc,uVar17,uVar15);
        if (7 < (uint)piVar13[5]) {
          piVar13 = (int *)*piVar13;
        }
        FUN_007aaab8(auStack_12c,auStack_47c,piVar13);
        FUN_007aa754(auStack_47c);
        FUN_00423f74(auStack_55c);
        local_144 = auStack_5b4;
        uVar15 = FUN_007aa7e4(auStack_fc,auStack_5b4);
        local_658 = auStack_5cc;
        uVar17 = FUN_00426e64(auStack_5cc,&DAT_0086f630);
        uVar12 = FUN_007aa5e0(auStack_12c);
        local_618 = 7;
        local_61c = 0;
        local_62c[0] = 0;
        FUN_00425168(local_62c,uVar12,0,0xffffffff);
        piVar13 = (int *)FUN_00426810(auStack_5e4,local_62c,uVar17,uVar15);
        if (7 < (uint)piVar13[5]) {
          piVar13 = (int *)*piVar13;
        }
        FUN_007aaab8(auStack_12c,auStack_434,piVar13);
        FUN_007aa754(auStack_434);
        FUN_00423f74(auStack_5e4);
        goto LAB_0064be6e;
      }
    }
    FUN_006050e4(auStack_12c,uVar14,local_13c,local_14c,600);
    FUN_007aa754(auStack_12c);
    break;
  case 0x17:
    puVar11 = (undefined1 *)FUN_00644250(&DAT_00fe1dd8);
    iVar16 = (int)puVar11 * 0x274 + DAT_00fe1fa8;
    local_14c = puVar11;
    iVar8 = FUN_006448f8();
    *(int *)(iVar16 + 0x1b4) = iVar8;
    if ((iVar8 != 0) && (*(char *)(iVar16 + 0x100) != '\0')) {
      iVar8 = FUN_006442ec(&DAT_00fe1dd8);
      puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_644);
      *(undefined4 *)(iVar16 + 0x138) = *puVar10;
      *(undefined4 *)(iVar16 + 0x13c) = puVar10[1];
      *(int *)(iVar16 + 0x148) = (int)*(float *)(iVar16 + 0x138);
      *(int *)(iVar16 + 0x14c) = (int)*(float *)(iVar16 + 0x13c);
      if (*(short *)(iVar16 + 0x1f0) != iVar8) {
        uVar14 = *(undefined4 *)(iVar16 + 0x104);
        FUN_00684770(iVar16,iVar8);
        FUN_0065b0d4(iVar16,uVar14,*(undefined4 *)(iVar16 + 0x104));
        puVar11 = local_14c;
      }
      if (iVar8 == 0x106) {
        puVar26 = &DAT_0090252c;
LAB_0064baa2:
        *puVar26 = (uint)puVar11;
      }
      else if (iVar8 == 0xf5) {
        puVar26 = &DAT_00902528;
        goto LAB_0064baa2;
      }
      puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_634);
      *(undefined4 *)(iVar16 + 0x140) = *puVar10;
      *(undefined4 *)(iVar16 + 0x144) = puVar10[1];
      bVar2 = FUN_00644250(&DAT_00fe1dd8);
      *(byte *)(iVar16 + 0x175) = bVar2 & 0xf;
      if ((bVar2 & 0x10) == 0) {
        uVar4 = 0xff;
      }
      else {
        uVar4 = 1;
      }
      uVar25 = 0xff;
      *(undefined1 *)(iVar16 + 0x171) = uVar4;
      if ((bVar2 & 0x20) == 0) {
        uVar4 = 0xff;
      }
      else {
        uVar4 = 1;
      }
      *(undefined1 *)(iVar16 + 0x172) = uVar4;
      if ((bVar2 & 0x40) != 0) {
        uVar25 = 1;
      }
      *(undefined1 *)(iVar16 + 0x1e1) = uVar25;
      uVar9 = FUN_00644250(&DAT_00fe1dd8);
      puVar11 = puVar19;
      if ((uVar9 & 1) != 0) {
        puVar11 = (undefined1 *)FUN_0064433c(&DAT_00fe1dd8);
      }
      *(undefined1 **)(iVar16 + 0x178) = puVar11;
      puVar11 = puVar19;
      if ((uVar9 & 2) != 0) {
        puVar11 = (undefined1 *)FUN_0064433c(&DAT_00fe1dd8);
      }
      *(undefined1 **)(iVar16 + 0x17c) = puVar11;
      puVar11 = puVar19;
      if ((uVar9 & 4) != 0) {
        puVar11 = (undefined1 *)FUN_0064433c(&DAT_00fe1dd8);
      }
      *(undefined1 **)(iVar16 + 0x180) = puVar11;
      puVar11 = puVar19;
      if ((uVar9 & 8) != 0) {
        puVar11 = (undefined1 *)FUN_0064433c(&DAT_00fe1dd8);
      }
      *(undefined1 **)(iVar16 + 0x184) = puVar11;
    }
    break;
  case 0x32:
    uVar9 = FUN_00644250(&DAT_00fe1dd8);
    local_14c = (undefined1 *)FUN_0058ef48();
    if ((*(int *)(local_14c + 0x10) != 0) && ((byte)local_14c[0x20] == uVar9)) {
      FUN_006451f4(&DAT_00902038,DAT_00902040,&local_14c);
      break;
    }
    if (6 < DAT_00fe1eb4) {
      DAT_00fe1eb4 = 8;
    }
    goto LAB_0064cc82;
  case 0x37:
    uVar9 = FUN_00644314(&DAT_00fe1dd8);
    iVar16 = 5;
    iVar8 = (uVar9 >> 6) * 0x274 + DAT_00fe1fa8;
    do {
      if ((uVar9 & 1) == 0) {
        *(undefined2 *)(iVar8 + 0x68) = 0;
        *(undefined2 *)(iVar8 + 0x6a) = 0;
      }
      else {
        uVar5 = FUN_00644250(&DAT_00fe1dd8);
        *(undefined2 *)(iVar8 + 0x68) = uVar5;
        uVar5 = FUN_006448f8();
        *(undefined2 *)(iVar8 + 0x6a) = uVar5;
      }
      uVar9 = uVar9 >> 1;
      iVar8 = iVar8 + 6;
      iVar16 = iVar16 + -1;
    } while (iVar16 != 0);
    break;
  case 0x39:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    uVar14 = FUN_0064a1d4(&DAT_00fe1dd8,auStack_4f4);
    FUN_007aa80c(&DAT_00fe1fb0 + iVar8 * 0x30,auStack_3ec,uVar14);
    FUN_007aa754(auStack_3ec);
    FUN_007aa754(auStack_4f4);
    break;
  case 0x3a:
    DAT_0102a70f = FUN_00644250(&DAT_00fe1dd8);
    DAT_0102a70e = FUN_00644250(&DAT_00fe1dd8);
    break;
  case 0x41:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = *(int *)((&DAT_0107034c)[iVar8] + 0x34);
    uVar14 = FUN_00644250(&DAT_00fe1dd8);
    if (iVar8 != 0) {
      FUN_00738154(iVar8,uVar14);
    }
    break;
  case 0x42:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = *(int *)((&DAT_0107034c)[iVar8] + 0x34);
    uVar14 = FUN_00644250(&DAT_00fe1dd8);
    if (iVar8 != 0) {
      iVar8 = FUN_0058ef48();
      FUN_00727e04(*(undefined4 *)(iVar8 + 0x3470),uVar14,1);
    }
    break;
  case 0x4a:
    iVar16 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = DAT_00fe1fa8;
    local_134 = 0;
    local_130 = 0;
    uVar14 = FUN_00644250(&DAT_00fe1dd8);
    local_134 = FUN_00644278(&DAT_00fe1dd8);
    local_130 = FUN_00644278(&DAT_00fe1dd8);
    FUN_00654608(iVar16 * 0x274 + iVar8,&local_134,uVar14);
    uVar7 = local_654;
    break;
  case 0x53:
    DAT_010703dc = FUN_0064433c(&DAT_00fe1dd8);
    uVar9 = FUN_00644250(&DAT_00fe1dd8);
    DAT_010703e0 = (uVar9 & 1) != 0;
    DAT_010703e2 = (uVar9 & 2) != 0;
    DAT_010703e4 = (undefined1)((int)uVar9 >> 2);
    break;
  case 0x54:
    FUN_007624d4();
    break;
  case 0x55:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    FUN_00644250(&DAT_00fe1dd8);
    uVar14 = FUN_006442ec(&DAT_00fe1dd8);
    FUN_00684770(iVar8 * 0x274 + DAT_00fe1fa8,uVar14);
    puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_63c);
    iVar16 = iVar8 * 0x274 + DAT_00fe1fa8;
    *(undefined4 *)(iVar16 + 0x138) = *puVar10;
    *(undefined4 *)(iVar16 + 0x13c) = puVar10[1];
    iVar16 = iVar8 * 0x274 + DAT_00fe1fa8;
    *(int *)(iVar16 + 0x148) = (int)*(float *)(iVar16 + 0x138);
    iVar8 = iVar8 * 0x274 + DAT_00fe1fa8;
    *(int *)(iVar8 + 0x14c) = (int)*(float *)(iVar8 + 0x13c);
    goto LAB_0064cc82;
  case 0x56:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 * 0x274 + DAT_00fe1fa8 + 0x100) = 0;
    *(undefined4 *)(iVar8 * 0x274 + DAT_00fe1fa8 + 0x1b4) = 0;
    goto LAB_0064cc82;
  }
switchD_0064b212_caseD_1:
switchD_0064c074_caseD_2:
LAB_0064cc82:
  uVar4 = 0xff;
  switch((byte)local_148[0]) {
  case 4:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar7 = FUN_00644314(&DAT_00fe1dd8);
    iVar8 = (&DAT_0107034c)[uVar7 & 3];
    *(byte *)(iVar8 + 0x8d78) = (byte)((int)uVar7 >> 4) & 0x3f;
    *(char *)(iVar8 + 0x5c80) = (char)((int)uVar7 >> 0xb);
    *(bool *)(iVar8 + 0xdb) = ((int)uVar7 >> 10 & 1U) != 0;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d5c) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d5d) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d5e) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d60) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d61) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d62) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d64) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d65) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d66) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d68) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d69) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d6a) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d6c) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d6d) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d6e) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d70) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d71) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d72) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d74) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d75) = uVar4;
    uVar4 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined1 *)(iVar8 + 0x8d76) = uVar4;
    FUN_007aa80c(iVar8 + 0x8e2c,auStack_4f4,iVar8 + 0x5c1c);
    FUN_007aa754(auStack_4f4);
    uVar14 = FUN_0064a1d4(&DAT_00fe1dd8,auStack_434);
    FUN_007aa80c(iVar8 + 0x5c1c,auStack_3ec,uVar14);
    FUN_007aa754(auStack_3ec);
    FUN_007aa754(auStack_434);
    FUN_007aa80c(iVar8 + 0x8e2c,auStack_47c,iVar8 + 0x5c1c);
    FUN_007aa754(auStack_47c);
    *(undefined4 *)(iVar8 + 0x5bd8) = 0;
    break;
  case 5:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar18 = (&DAT_0107034c)[iVar8];
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar16 = FUN_006448f8();
    uVar15 = 0;
    uVar14 = 0;
    if (0 < iVar16) {
      uVar14 = FUN_00644250(&DAT_00fe1dd8);
      uVar15 = FUN_006442ec(&DAT_00fe1dd8);
    }
    if (iVar8 < 0x30) {
      iVar8 = iVar18 + iVar8 * 0xa0 + 0x948;
    }
    else {
      iVar8 = iVar8 + -0x30;
      if (iVar8 < 0xb) {
        iVar8 = iVar18 + iVar8 * 0xa0 + 0x268;
      }
      else {
        iVar8 = iVar18 + iVar8 * 0xa0 + 0x2108;
      }
    }
    if (iVar16 < 1) {
      FUN_00612a6c(iVar8);
    }
    else {
      FUN_006317fc(iVar8,uVar15,iVar16);
      FUN_00613df4(iVar8,uVar14);
    }
    break;
  case 0xc:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar16 = (&DAT_0107034c)[iVar8];
    if (DAT_00fe1ec4 == 1) {
      iVar18 = FUN_006becfc(iVar16);
      if (iVar18 == 0) {
        FUN_006becfc(iVar16);
      }
      if (*(int *)(iVar16 + 0x34) != 0) {
        FUN_00739ad8(*(int *)(iVar16 + 0x34),0);
      }
    }
    else {
      FUN_00644d18(uVar7);
    }
    uVar14 = FUN_006442ec(&DAT_00fe1dd8);
    *(undefined4 *)(iVar16 + 0x5d98) = uVar14;
    uVar14 = FUN_006442ec(&DAT_00fe1dd8);
    *(undefined4 *)(iVar16 + 0x5d9c) = uVar14;
    FUN_006d8a34(iVar16);
    if ((DAT_00fe1ec4 == 2) && (2 < *(short *)(uVar7 + 0x10))) {
      if (*(short *)(uVar7 + 0x10) == 3) {
        *(undefined2 *)(uVar7 + 0x10) = 10;
        FUN_0064a230();
        FUN_0064afe0();
      }
      else {
        FUN_0064ab90(iVar8);
      }
    }
    break;
  case 0xd:
    uVar9 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = (&DAT_0107034c)[uVar9 & 0x1f];
    if ((uVar9 & 0x40) != 0) {
      uVar4 = 1;
    }
    *(undefined1 *)(iVar8 + 0x5cc8) = uVar4;
    if ((uVar9 & 0x80) == 0) {
      uVar32 = 0;
    }
    else {
      uVar32 = FUN_00644250(&DAT_00fe1dd8);
    }
    *(bool *)(iVar8 + 0x5cb6) = (uVar32 & 1) != 0;
    *(bool *)(iVar8 + 0x5cb8) = (uVar32 & 2) != 0;
    *(bool *)(iVar8 + 0x5cb0) = (uVar32 & 4) != 0;
    *(bool *)(iVar8 + 0x5cb2) = (uVar32 & 8) != 0;
    *(bool *)(iVar8 + 0x5cb9) = (uVar32 & 0x10) != 0;
    *(bool *)(iVar8 + 0x5cba) = (uVar32 & 0x20) != 0;
    *(bool *)(iVar8 + 0x67) = (uVar32 & 0x40) != 0;
    if ((uVar32 & 0x80) == 0) {
      uVar4 = 1;
    }
    else {
      uVar4 = 2;
    }
    *(undefined1 *)(iVar8 + 0x66) = uVar4;
    if ((uVar9 & 0x20) != 0) {
      uVar4 = FUN_00644228(&DAT_00fe1dd8);
      *(undefined1 *)(iVar8 + 0x14c) = uVar4;
    }
    puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_634);
    *(undefined4 *)(iVar8 + 0x110) = *puVar10;
    *(undefined4 *)(iVar8 + 0x114) = puVar10[1];
    *(int *)(iVar8 + 0x100) = (int)*(float *)(iVar8 + 0x110);
    *(int *)(iVar8 + 0x104) = (int)*(float *)(iVar8 + 0x114);
    puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_644);
    *(undefined4 *)(iVar8 + 0x120) = *puVar10;
    *(undefined4 *)(iVar8 + 0x124) = puVar10[1];
    *(short *)(iVar8 + 0x8dec) = (short)(*(int *)(iVar8 + 0x104) >> 4);
    if ((DAT_00fe1ec4 == 2) && (*(short *)(uVar7 + 0x10) == 10)) {
      FUN_00644d18(uVar7);
    }
    break;
  case 0xf:
    iVar8 = FUN_00644314(&DAT_00fe1dd8);
    iVar16 = FUN_00644314(&DAT_00fe1dd8);
    FUN_00730d74((iVar8 * DAT_00902934 + iVar16) * 0xe + DAT_00902928,&DAT_00fe1dd8);
    FUN_0076e528(iVar8,iVar16,0);
    FUN_0074dfcc(iVar8,iVar16,0);
    if (DAT_00fe1ec4 == 2) {
      FUN_00646500(0xf,iVar8,iVar16);
      FUN_00644da0(uVar7);
    }
    break;
  case 0x10:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar16 = (&DAT_0107034c)[iVar8];
    iVar8 = FUN_006448f8();
    *(short *)(iVar16 + 0x5d02) = (short)iVar8;
    sVar6 = *(short *)(iVar16 + 0x5d00);
    if (*(short *)(iVar16 + 0x5d00) <= iVar8) {
      sVar6 = (short)iVar8;
    }
    *(short *)(iVar16 + 0x5d00) = sVar6;
    break;
  case 0x11:
    puVar19 = (undefined1 *)FUN_00644250(&DAT_00fe1dd8);
    local_658 = puVar19;
    iVar8 = FUN_00644314(&DAT_00fe1dd8);
    iVar16 = FUN_00644314(&DAT_00fe1dd8);
    if ((int)puVar19 < 5) {
      local_65c = FUN_00644314(&DAT_00fe1dd8);
      if (local_65c == 1) goto LAB_0064d210;
      local_654 = 0;
    }
    else {
      local_65c = 1;
LAB_0064d210:
      local_654 = 1;
    }
    if (DAT_00fe1ec4 == 2) {
      if ((local_654 == 0) &&
         ((((puVar19 == (undefined1 *)0x0 || (puVar19 == (undefined1 *)0x2)) ||
           (puVar19 == (undefined1 *)0x4)) &&
          (iVar18 = (int)((ulonglong)((longlong)iVar8 * (longlong)DAT_0064d294) >> 0x20),
          iVar22 = (int)((ulonglong)((longlong)iVar16 * (longlong)DAT_0064d290) >> 0x20) + iVar16,
          puVar19 = local_658,
          *(char *)(((iVar18 >> 3) - (iVar18 >> 0x1f)) * *(int *)(uVar7 + 0x20) +
                    ((iVar22 >> 3) - (iVar22 >> 0x1f)) + *(int *)(uVar7 + 0x14)) == '\0')))) {
        local_654 = 1;
      }
      FUN_00644d18(uVar7);
    }
    switch(puVar19) {
    case (undefined1 *)0x0:
      FUN_007743f8(iVar8,iVar16,local_654,0,0,0);
      break;
    case (undefined1 *)0x1:
      uVar14 = FUN_00644250(&DAT_00fe1dd8);
      FUN_007749e4(iVar8,iVar16,local_65c,0,1,0xffffffff,uVar14);
      if ((local_65c == 0x35) && (DAT_00fe1ec4 == 2)) {
        FUN_00646500(0xf,iVar8,iVar16);
        FUN_00644ee4();
      }
      break;
    case (undefined1 *)0x2:
      FUN_00772a70(iVar8,iVar16);
      break;
    case (undefined1 *)0x3:
      FUN_0075b374(iVar8,iVar16,local_65c);
      break;
    case (undefined1 *)0x4:
      FUN_007743f8(iVar8,iVar16,local_654,0,1,0);
      break;
    case (undefined1 *)0x5:
      FUN_00752628(iVar8,iVar16,0);
      break;
    case (undefined1 *)0x6:
      FUN_0075250c(iVar8,iVar16,0);
      break;
    case (undefined1 *)0x7:
      FUN_00777168(iVar8,iVar16);
      break;
    case (undefined1 *)0x8:
      FUN_00752788(iVar8,iVar16);
      break;
    case (undefined1 *)0x9:
      FUN_007526b4(iVar8,iVar16,local_654);
      break;
    case (undefined1 *)0xa:
      FUN_00752628(iVar8,iVar16,1);
      break;
    case (undefined1 *)0xb:
      FUN_0075250c(iVar8,iVar16,1);
      break;
    case (undefined1 *)0xc:
      FUN_00752628(iVar8,iVar16,2);
      break;
    case (undefined1 *)0xd:
      FUN_0075250c(iVar8,iVar16,2);
      break;
    case (undefined1 *)0xe:
      FUN_00777238(iVar8,iVar16,0);
    }
    break;
  case 0x13:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar14 = FUN_00644314(&DAT_00fe1dd8);
    uVar15 = FUN_00644314(&DAT_00fe1dd8);
    uVar17 = FUN_00644228(&DAT_00fe1dd8);
    FUN_00776dc8(uVar14,uVar15,uVar17);
    break;
  case 0x14:
    puVar27 = (undefined *)FUN_00644250(&DAT_00fe1dd8);
    local_13c = puVar27;
    puVar11 = (undefined1 *)FUN_00644314(&DAT_00fe1dd8);
    local_14c = puVar11;
    puVar19 = (undefined1 *)FUN_00644314(&DAT_00fe1dd8);
    puVar24 = puVar11 + (int)puVar27;
    local_658 = puVar19;
    if ((int)puVar11 < (int)puVar24) {
      puVar30 = puVar19 + (int)puVar27;
      puVar1 = puVar19;
      uVar7 = local_654;
      puVar27 = local_13c;
      puVar29 = puVar11;
      do {
        for (; puVar11 = local_14c, local_654 = uVar7, local_14c = puVar11, local_13c = puVar27,
            (int)puVar1 < (int)puVar30; puVar1 = puVar1 + 1) {
          FUN_00730d74((int)(puVar1 + DAT_00902934 * (int)puVar29) * 0xe + DAT_00902928,
                       &DAT_00fe1dd8);
          puVar19 = local_658;
          uVar7 = local_654;
          puVar27 = local_13c;
        }
        puVar29 = puVar29 + 1;
        puVar1 = puVar19;
      } while ((int)puVar29 < (int)puVar24);
    }
    FUN_007728b0(puVar11,puVar19,puVar24,puVar19 + (int)puVar27);
    if (DAT_00fe1ec4 == 2) {
      EnterCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e5c);
      DAT_00fe1e74 = 1;
      (**(code **)(DAT_00fe1d38 + 0x24))(&DAT_00fe1d38);
      local_148[0]._0_1_ = 0x14;
      (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_148,1);
      local_148[0] = CONCAT11(local_148[0]._1_1_,(char)puVar27);
      (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_148,1);
      local_148[0] = (short)puVar11;
      (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_148,2);
      local_148[0] = (short)local_658;
      (**(code **)(DAT_00fe1d38 + 0x18))(&DAT_00fe1d38,local_148,2);
      puVar1 = local_658;
      for (; puVar29 = puVar1, (int)puVar11 < (int)puVar24; puVar11 = puVar11 + 1) {
        for (; (int)puVar29 < (int)(puVar19 + (int)puVar27); puVar29 = puVar29 + 1) {
          FUN_00730f00((int)(puVar29 + (int)puVar11 * DAT_00902934) * 0xe + DAT_00902928,
                       &DAT_00fe1d38);
          puVar1 = local_658;
        }
        uVar7 = local_654;
      }
      LeaveCriticalSection((LPCRITICAL_SECTION)&DAT_00fe1e5c);
      DAT_00fe1e74 = 0;
      FUN_00644da0(uVar7);
    }
    break;
  case 0x15:
    iVar18 = FUN_00644250(&DAT_00fe1dd8);
    local_14c = (undefined1 *)(iVar18 * 5);
    iVar8 = iVar18 * 0xa0;
    uVar7 = FUN_00644278(&DAT_00fe1dd8);
    local_654 = (uint)(byte)(&DAT_010731dc)[iVar8];
    uVar9 = uVar7 & 0xf;
    iVar16 = (int)uVar7 >> 4;
    if (DAT_00fe1ec4 == 1) {
      if (iVar16 == 0) {
        (&DAT_010731a8)[iVar8] = 0;
      }
      else {
        FUN_006317fc(&DAT_010731a0 + iVar8,iVar16,1);
        bVar2 = FUN_00644250(&DAT_00fe1dd8);
        if ((bVar2 & 0x80) != 0) {
          bVar2 = bVar2 ^ 0x80;
          uVar14 = FUN_00644250(&DAT_00fe1dd8);
          FUN_00613df4(&DAT_010731a0 + iVar8,uVar14);
        }
        (&DAT_010731cd)[iVar8] = bVar2;
        uVar5 = FUN_006448f8();
        *(undefined2 *)(&DAT_010731e2 + iVar8) = uVar5;
        puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_63c);
        (&DAT_010731f8)[iVar18 * 0x28] = *puVar10;
        (&DAT_010731fc)[iVar18 * 0x28] = puVar10[1];
        puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_564);
        (&DAT_01073200)[iVar18 * 0x28] = *puVar10;
        (&DAT_01073204)[iVar18 * 0x28] = puVar10[1];
        uVar4 = FUN_0060a19c(&DAT_010731f8 + (int)local_14c * 8,
                             (&DAT_010731f4)[(int)local_14c * 0x10],
                             (&DAT_010731f6)[(int)local_14c * 0x10]);
        (&DAT_010731ac)[iVar8] = uVar4;
        (&DAT_010731dc)[iVar8] = (char)local_654;
      }
    }
    else if (iVar16 == 0) {
      (&DAT_010731a8)[iVar8] = 0;
      FUN_0064998c(uVar9,iVar18,0);
    }
    else {
      uVar7 = FUN_00644250(&DAT_00fe1dd8);
      local_65c = 0;
      if ((uVar7 & 0x80) != 0) {
        uVar7 = uVar7 ^ 0x80;
        local_65c = FUN_00644250(&DAT_00fe1dd8);
      }
      puVar19 = (undefined1 *)FUN_006448f8();
      local_14c = puVar19;
      fVar35 = (float)FUN_0064433c(&DAT_00fe1dd8);
      fVar34 = (float)FUN_0064433c(&DAT_00fe1dd8);
      local_658 = (undefined1 *)(uint)(iVar18 == 200);
      if (local_658 == (undefined1 *)0x0) {
        (&DAT_010731f8)[iVar18 * 0x28] = fVar35;
        (&DAT_010731fc)[iVar18 * 0x28] = fVar34;
      }
      else {
        FUN_00613968(auStack_cc);
        FUN_006317fc(auStack_cc,iVar16,puVar19);
        sVar6 = FUN_00631590((int)fVar35,(int)fVar34,local_78,local_76,local_c8,puVar19,1,0,0);
        iVar18 = (int)sVar6;
      }
      FUN_006317fc(&DAT_010731a0 + iVar18 * 0xa0,iVar16,local_14c);
      FUN_00613df4(&DAT_010731a0 + iVar18 * 0xa0,local_65c);
      iVar8 = iVar18 * 0xa0;
      (&DAT_010731cd)[iVar8] = (char)uVar7;
      puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_594);
      (&DAT_01073200)[iVar18 * 0x28] = *puVar10;
      (&DAT_01073204)[iVar18 * 0x28] = puVar10[1];
      FUN_0064998c(uVar9,iVar18,2);
      if (local_658 == (undefined1 *)0x0) {
        (&DAT_010731dc)[iVar8] = (char)local_654;
        FUN_00644da0(*(undefined4 *)((&DAT_0107034c)[uVar9] + 0x30));
      }
      else {
        FUN_00644ee4();
        if (uVar7 == 0) {
          (&DAT_010731dd)[iVar8] = (char)uVar9;
          (&DAT_010731de)[iVar8] = 100;
        }
      }
    }
    break;
  case 0x16:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    uVar9 = FUN_00644250(&DAT_00fe1dd8);
    uVar32 = uVar9 & 0x80 ^ uVar9;
    (&DAT_010731dc)[iVar8 * 0xa0] = (char)uVar32;
    if ((int)uVar32 < 4) {
      (&DAT_010731df)[iVar8 * 0xa0] = 0xf;
      puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_59c);
      (&DAT_010731f8)[iVar8 * 0x28] = *puVar10;
      (&DAT_010731fc)[iVar8 * 0x28] = puVar10[1];
      if ((uVar9 & 0x80) == 0) {
        (&DAT_01073200)[iVar8 * 0x28] = puVar19;
        (&DAT_01073204)[iVar8 * 0x28] = puVar19;
      }
      else {
        puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_53c);
        (&DAT_01073200)[iVar8 * 0x28] = *puVar10;
        (&DAT_01073204)[iVar8 * 0x28] = puVar10[1];
      }
    }
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    break;
  case 0x18:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar14 = FUN_00644314(&DAT_00fe1dd8);
    uVar15 = FUN_00644314(&DAT_00fe1dd8);
    FUN_00772624(uVar14,uVar15,1);
    break;
  case 0x1a:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar7 = FUN_00644250(&DAT_00fe1dd8);
    uVar14 = FUN_00644228(&DAT_00fe1dd8);
    uVar15 = FUN_006442ec(&DAT_00fe1dd8);
    FUN_0064429c(&DAT_00fe1dd8);
    FUN_006d6464((&DAT_0107034c)[uVar7 & 3],uVar15,uVar14,(uVar7 & 0x40) != 0,1);
    break;
  case 0x1b:
    uVar9 = FUN_00644250(&DAT_00fe1dd8);
    local_14c = (undefined1 *)((int)uVar9 >> 4);
    uVar32 = FUN_006448f8();
    uVar20 = FUN_006448f8();
    puVar33 = &DAT_0107af40;
    bVar3 = false;
    iVar8 = 0x200;
    iVar16 = 0;
    puVar21 = puVar33;
    do {
      if ((((ushort)puVar21[1] == uVar20) && ((uint)*(byte *)(puVar21 + 0x11) == (uVar9 & 0xf))) &&
         (*(char *)(puVar21 + 2) != '\0')) {
        bVar3 = true;
        iVar8 = iVar16;
      }
      iVar16 = iVar16 + 1;
      puVar21 = puVar21 + 0x74;
    } while (iVar16 < 0x200);
    iVar16 = iVar8;
    if (!bVar3) {
      iVar18 = 0;
      do {
        iVar16 = iVar18;
        if (*(char *)(puVar33 + 2) == '\0') break;
        iVar18 = iVar18 + 1;
        puVar33 = puVar33 + 0x74;
        iVar16 = iVar8;
      } while (iVar18 < 0x200);
    }
    iVar8 = iVar16 * 0xe8;
    puVar31 = &DAT_0107af40 + iVar16 * 0x74;
    if (((&DAT_0107af44)[iVar8] == '\0') || (*puVar31 != uVar32)) {
      FUN_006f0e64(puVar31);
    }
    (&DAT_0107af62)[iVar8] = (char)(uVar9 & 0xf);
    (&DAT_0107af42)[iVar16 * 0x74] = (short)uVar20;
    puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_584);
    (&DAT_0107af84)[iVar16 * 0x3a] = *puVar10;
    (&DAT_0107af88)[iVar16 * 0x3a] = puVar10[1];
    fVar35 = (float)(&DAT_0107af88)[iVar16 * 0x3a];
    *(int *)(&DAT_0107af6c + iVar8) = (int)(float)(&DAT_0107af84)[iVar16 * 0x3a];
    *(int *)(&DAT_0107af70 + iVar8) = (int)fVar35;
    puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_544);
    puVar11 = local_14c;
    *(undefined4 *)(&DAT_0107af94 + iVar8) = *puVar10;
    *(undefined4 *)(&DAT_0107af98 + iVar8) = puVar10[1];
    if (((uint)local_14c & 1) == 0) {
      *(undefined1 **)(&DAT_0107af7c + iVar8) = puVar19;
    }
    else {
      (**(code **)(DAT_00fe1dd8 + 0x1c))(&DAT_00fe1dd8,&local_14c,4);
      *(undefined1 **)(&DAT_0107af7c + iVar8) = local_14c;
    }
    if (((uint)puVar11 & 2) == 0) {
      sVar6 = 0;
    }
    else {
      (**(code **)(DAT_00fe1dd8 + 0x1c))(&DAT_00fe1dd8,local_148,2);
      sVar6 = local_148[0];
    }
    *(short *)(&DAT_0107afc2 + iVar8) = sVar6;
    puVar24 = puVar19;
    if (((uint)puVar11 & 4) != 0) {
      (**(code **)(DAT_00fe1dd8 + 0x1c))(&DAT_00fe1dd8,&local_14c,4);
      puVar24 = local_14c;
    }
    (&DAT_0107afb4)[iVar16 * 0x3a] = puVar24;
    if (((uint)puVar11 & 8) != 0) {
      (**(code **)(DAT_00fe1dd8 + 0x1c))(&DAT_00fe1dd8,&local_14c,4);
      puVar19 = local_14c;
    }
    (&DAT_0107afb8)[iVar16 * 0x3a] = (int)(float)puVar19;
    if (DAT_00fe1ec4 == 2) {
      FUN_00644c88(uVar7,puVar31);
    }
    break;
  case 0x1c:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar16 = FUN_006442ec(&DAT_00fe1dd8);
    if (iVar16 < 0) {
      *(undefined4 *)(iVar8 * 0x274 + DAT_00fe1fa8 + 0x1b4) = 0;
      iVar16 = iVar8 * 0x274 + DAT_00fe1fa8;
      if (*(char *)(iVar16 + 0x100) == '\0') break;
      FUN_00697e24(0x41200000,iVar16,0);
      *(undefined1 *)(iVar8 * 0x274 + DAT_00fe1fa8 + 0x100) = 0;
    }
    else {
      uVar14 = FUN_0064433c(&DAT_00fe1dd8);
      uVar7 = FUN_00644228(&DAT_00fe1dd8);
      FUN_006b4dbc(uVar14,iVar8 * 0x274 + DAT_00fe1fa8,iVar16,(int)uVar7 >> 1,(uVar7 & 1) != 0,0);
    }
    if (DAT_00fe1ec4 == 2) {
      iVar16 = iVar8 * 0x274 + DAT_00fe1fa8;
      if (*(int *)(iVar16 + 0x1b4) < 1) {
        FUN_00649e80(iVar8,0);
      }
      else {
        *(undefined1 *)(iVar16 + 0x10c) = 1;
      }
    }
    break;
  case 0x1d:
    uVar9 = FUN_006448f8();
    uVar32 = FUN_00644250(&DAT_00fe1dd8);
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    puVar21 = &DAT_0107af40;
    iVar8 = 0;
    do {
      if (((*(byte *)(puVar21 + 0x11) == uVar32) && ((ushort)puVar21[1] == uVar9)) &&
         (*(char *)(puVar21 + 2) != '\0')) {
        FUN_006fde70(&DAT_0107af40 + iVar8 * 0x74);
        break;
      }
      iVar8 = iVar8 + 1;
      puVar21 = puVar21 + 0x74;
    } while (iVar8 < 0x200);
    break;
  case 0x1e:
    uVar9 = FUN_00644250(&DAT_00fe1dd8);
    uVar32 = uVar9 & 0x80;
    iVar8 = (&DAT_0107034c)[uVar32 ^ uVar9];
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
      if (uVar32 == 0) {
        uVar14 = 0x19;
      }
      else {
        uVar14 = 0x18;
      }
      uVar15 = FUN_00595220(auStack_20c,iVar8 + 0x5c1c);
      iVar16 = (uint)*(byte *)(iVar8 + 0x142) * 4;
      FUN_0064a758(uVar15,uVar14,*(undefined1 *)(&DAT_01070338 + *(byte *)(iVar8 + 0x142)),
                   *(undefined1 *)((int)&DAT_01070338 + iVar16 + 1),
                   *(undefined1 *)((int)&DAT_01070338 + iVar16 + 2),0xffffffff);
    }
    *(bool *)(iVar8 + 0x8d79) = uVar32 != 0;
    break;
  case 0x20:
    iVar8 = FUN_006448f8();
    iVar16 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = iVar8 * 0x1908 + DAT_00fd643c;
    if (*(char *)(iVar8 + 0x1904) != '\0') {
      *(undefined1 *)(iVar8 + 0x1904) = 0;
    }
    iVar18 = FUN_006442ec(&DAT_00fe1dd8);
    if (iVar18 == 0) {
      FUN_00612a6c(iVar8 + iVar16 * 0xa0);
    }
    else {
      uVar14 = FUN_00644250(&DAT_00fe1dd8);
      uVar15 = FUN_006448f8();
      FUN_006317fc(iVar8 + iVar16 * 0xa0,iVar18,uVar15);
      FUN_00613df4(iVar8 + iVar16 * 0xa0,uVar14);
    }
    break;
  case 0x21:
    uVar7 = FUN_006442ec(&DAT_00fe1dd8);
    iVar18 = (int)uVar7 >> 5;
    iVar16 = (&DAT_0107034c)[uVar7 & 0x1f];
    iVar8 = FUN_006becfc(iVar16);
    if (iVar8 == 0) {
      *(short *)(iVar16 + 0x8de2) = (short)iVar18;
      if (-1 < iVar18) {
        FUN_006104a0(&DAT_00fe1dd8,4);
      }
    }
    else {
      sVar6 = *(short *)(iVar16 + 0x8de2);
      *(short *)(iVar16 + 0x8de2) = (short)iVar18;
      if (-1 < iVar18) {
        uVar5 = FUN_006442ec(&DAT_00fe1dd8);
        *(undefined2 *)(iVar16 + 0x8de4) = uVar5;
        uVar5 = FUN_006442ec(&DAT_00fe1dd8);
        *(undefined2 *)(iVar16 + 0x8de6) = uVar5;
      }
      if (sVar6 == -1) {
        FUN_0073b56c(*(undefined4 *)(iVar16 + 0x34));
        FUN_007a7bf4(10);
      }
      else if (iVar18 == -1) {
        if (*(int *)(*(int *)(iVar16 + 0x34) + 0x2c78) == 2) {
          FUN_0073b540();
          FUN_007a7bf4(0xb);
        }
      }
      else if (sVar6 != iVar18) {
        FUN_0073b56c(*(undefined4 *)(iVar16 + 0x34));
        FUN_007a7bf4(0xc);
      }
    }
    break;
  case 0x23:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    uVar15 = (&DAT_0107034c)[iVar8];
    uVar14 = FUN_006448f8();
    FUN_006c0368(uVar15,uVar14,0);
    break;
  case 0x24:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar16 = (&DAT_0107034c)[iVar8];
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    *(byte *)(iVar16 + 0xe6) = (byte)iVar8 & 1;
    *(byte *)(iVar16 + 0xe8) = (byte)(iVar8 >> 1) & 1;
    *(byte *)(iVar16 + 0xe5) = (byte)(iVar8 >> 2) & 1;
    *(byte *)(iVar16 + 0xe9) = (byte)(iVar8 >> 3) & 1;
    *(byte *)(iVar16 + 0xe7) = (byte)(iVar8 >> 4) & 1;
    *(byte *)(iVar16 + 0xea) = (byte)(iVar8 >> 5) & 1;
    *(byte *)(iVar16 + 0xeb) = (byte)(iVar8 >> 6) & 1;
    *(byte *)(iVar16 + 0xec) = (byte)(iVar8 >> 7) & 1;
    break;
  case 0x29:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = (&DAT_0107034c)[iVar8];
    uVar5 = FUN_006442ec(&DAT_00fe1dd8);
    *(undefined2 *)(iVar8 + 0x8df0) = uVar5;
    break;
  case 0x2a:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = (&DAT_0107034c)[iVar8];
    uVar14 = FUN_0064433c(&DAT_00fe1dd8);
    *(undefined4 *)(iVar8 + 0x164) = uVar14;
    uVar5 = FUN_006442ec(&DAT_00fe1dd8);
    *(undefined2 *)(iVar8 + 0x154) = uVar5;
    *(undefined1 *)(iVar8 + 0x5cf9) =
         *(undefined1 *)(iVar8 + *(char *)(iVar8 + 0x14c) * 0xa0 + 0x95e);
    break;
  case 0x2b:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar7 = FUN_00644278(&DAT_00fe1dd8);
    iVar8 = (&DAT_0107034c)[uVar7 & 0xf];
    *(short *)(iVar8 + 0x5d06) = (short)uVar7 >> 4;
    *(short *)(iVar8 + 0x5d08) = (short)(uVar7 >> 0x10);
    break;
  case 0x2c:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    uVar14 = FUN_006442ec(&DAT_00fe1dd8);
    FUN_006c032c((&DAT_0107034c)[iVar8],uVar14);
    break;
  case 0x2d:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    uVar14 = FUN_00644228(&DAT_00fe1dd8);
    uVar15 = FUN_006442ec(&DAT_00fe1dd8);
    iVar16 = FUN_00644250(&DAT_00fe1dd8);
    uVar36 = FUN_0064429c(&DAT_00fe1dd8);
    uVar15 = VectorSignedToFloat(uVar15,(byte)(in_fpscr >> 0x16) & 3);
    FUN_006d3458(uVar15,(&DAT_0107034c)[iVar8],uVar14,iVar16 != 0,(&DAT_0107034c)[iVar8],uVar36);
    break;
  case 0x2e:
    uVar32 = FUN_00644250(&DAT_00fe1dd8);
    uVar9 = (int)uVar32 >> 4;
    iVar8 = (&DAT_0107034c)[uVar32 & 0xf];
    bVar2 = *(byte *)(iVar8 + 0x142);
    *(char *)(iVar8 + 0x142) = (char)uVar9;
    if (bVar2 != uVar9) {
      if (DAT_00fe1ec4 == 2) {
        FUN_00644d18(uVar7);
      }
      FUN_00595220(auStack_12c,iVar8 + 0x5c1c);
      FUN_00595220(auStack_fc,&DAT_00fd6948 + (uVar9 + 0x1a) * 0x30);
      local_144 = auStack_5e4;
      uVar14 = FUN_007aa7e4(auStack_12c,auStack_5e4);
      local_14c = auStack_5cc;
      uVar15 = FUN_00426e64(auStack_5cc,L"#PlayerName#");
      uVar17 = FUN_007aa5e0(auStack_fc);
      uVar17 = FUN_00425930(auStack_55c,uVar17);
      piVar13 = (int *)FUN_00426810(auStack_5b4,uVar17,uVar15,uVar14);
      if (7 < (uint)piVar13[5]) {
        piVar13 = (int *)*piVar13;
      }
      FUN_007aaab8(auStack_fc,auStack_4c4,piVar13);
      FUN_007aa754(auStack_4c4);
      FUN_00423f74(auStack_5b4);
      FUN_006050e4(auStack_fc,*(undefined1 *)(&DAT_01070338 + uVar9),
                   *(undefined1 *)((int)&DAT_01070338 + uVar9 * 4 + 1),
                   *(undefined1 *)((int)&DAT_01070338 + uVar9 * 4 + 2),600);
      FUN_007aa754(auStack_fc);
      FUN_007aa754(auStack_12c);
    }
    break;
  case 0x30:
    FUN_00644250(&DAT_00fe1dd8);
    iVar8 = FUN_006448f8();
    FUN_00725fe8(iVar8 * 0x38 + DAT_009027f0,&DAT_00fe1dd8);
    break;
  case 0x31:
    uVar7 = FUN_00644314(&DAT_00fe1dd8);
    uVar9 = uVar7 & 0xffff3fff;
    local_14c = (undefined1 *)FUN_00644314(&DAT_00fe1dd8);
    uVar32 = (uint)local_14c & 0xffff3fff;
    if ((uVar7 & 0x8000) == 0) {
      iVar8 = uVar9 * DAT_00902934 + uVar32;
      if ((uVar7 & 0x4000) == 0) {
        *(undefined1 *)(iVar8 * 0xe + DAT_00902928 + 4) = 0;
      }
      else {
        *(undefined1 *)(iVar8 * 0xe + DAT_00902928 + 4) = 0xff;
      }
    }
    else {
      uVar4 = FUN_00644250(&DAT_00fe1dd8);
      *(undefined1 *)((uVar9 * DAT_00902934 + uVar32) * 0xe + DAT_00902928 + 4) = uVar4;
    }
    iVar8 = (uVar9 * DAT_00902934 + uVar32) * 0xe + DAT_00902928;
    *(ushort *)(iVar8 + 2) =
         *(ushort *)(iVar8 + 2) ^
         (*(ushort *)(iVar8 + 2) ^ (ushort)(((int)local_14c >> 0xe) << 0xc)) & 0x3000;
    if (DAT_00fe1ec4 == 2) {
      FUN_007729e8(uVar9,uVar32,0xffffffff);
    }
    break;
  case 0x33:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = (&DAT_0107034c)[iVar8];
    uVar7 = FUN_00644314(&DAT_00fe1dd8);
    uVar9 = 0;
    do {
      if ((1 << (uVar9 & 0xff) & uVar7) == 0) {
        *(undefined2 *)(iVar8 + 0x17c) = 0;
        *(undefined2 *)(iVar8 + 0x17e) = 0;
      }
      else {
        uVar5 = FUN_00644250(&DAT_00fe1dd8);
        *(undefined2 *)(iVar8 + 0x17c) = uVar5;
        *(undefined2 *)(iVar8 + 0x17e) = 0x3c;
      }
      uVar9 = uVar9 + 1;
      iVar8 = iVar8 + 6;
    } while ((int)uVar9 < 10);
    break;
  case 0x34:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    FUN_007a8384(2,*(undefined4 *)((&DAT_0107034c)[iVar8] + 0x100),
                 *(undefined4 *)((&DAT_0107034c)[iVar8] + 0x104),1);
    break;
  case 0x35:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    FUN_00644250(&DAT_00fe1dd8);
    uVar14 = FUN_00644314(&DAT_00fe1dd8);
    uVar15 = FUN_00644314(&DAT_00fe1dd8);
    FUN_00605890(uVar14,uVar15);
    if (DAT_00fe1ec4 == 2) {
      FUN_006496ec(uVar14,uVar15,2);
    }
    break;
  case 0x38:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    uVar17 = (&DAT_0107034c)[iVar8];
    uVar14 = FUN_00644250(&DAT_00fe1dd8);
    uVar15 = FUN_006448f8();
    FUN_006c48cc(uVar17,uVar14,uVar15,1);
    break;
  case 0x3b:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = (&DAT_0107034c)[iVar8];
    DAT_0103397c = FUN_0064433c(&DAT_00fe1dd8);
    if (*(int *)(iVar8 + *(char *)(iVar8 + 0x14c) * 0xa0 + 0x94c) == 0x1fb) {
      uVar14 = 0x23;
    }
    else {
      uVar14 = 0x1a;
    }
    FUN_007a8384(2,*(undefined4 *)(iVar8 + 0x100),*(undefined4 *)(iVar8 + 0x104),uVar14);
    break;
  case 0x3c:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar14 = FUN_00644314(&DAT_00fe1dd8);
    uVar15 = FUN_00644314(&DAT_00fe1dd8);
    FUN_00772d30(uVar14,uVar15);
    break;
  case 0x3d:
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    uVar14 = FUN_006442ec(&DAT_00fe1dd8);
    uVar15 = FUN_006442ec(&DAT_00fe1dd8);
    iVar16 = FUN_006442c4(&DAT_00fe1dd8);
    if (DAT_00fe1ec4 == 1) {
      *(char *)(iVar8 * 0x274 + DAT_00fe1fa8 + 0x119) = (char)iVar16;
      *(short *)(iVar8 * 0x274 + DAT_00fe1fa8 + 0x1f2) = (short)uVar14;
      *(short *)(iVar8 * 0x274 + DAT_00fe1fa8 + 500) = (short)uVar15;
    }
    else if (iVar16 == 0) {
      FUN_0074bd1c(iVar8);
    }
    else {
      FUN_00761ac8(uVar14,uVar15,iVar8);
    }
    break;
  case 0x45:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    FUN_00644250(&DAT_00fe1dd8);
    uVar14 = FUN_00644314(&DAT_00fe1dd8);
    uVar15 = FUN_00644314(&DAT_00fe1dd8);
    FUN_00755fb4(uVar14,uVar15);
    if (DAT_00fe1ec4 == 2) {
      FUN_006496ec(uVar14,uVar15,2);
    }
    break;
  case 0x46:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar7 = FUN_00644250(&DAT_00fe1dd8);
    if ((int)uVar7 >> 4 == 1) {
      FUN_006c2824((&DAT_0107034c)[uVar7 & 0xf]);
    }
    else if ((int)uVar7 >> 4 == 2) {
      FUN_006c4a44((&DAT_0107034c)[uVar7 & 0xf]);
    }
    break;
  case 0x47:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar14 = FUN_00644250(&DAT_00fe1dd8);
    uVar15 = FUN_006442ec(&DAT_00fe1dd8);
    uVar17 = FUN_006442ec(&DAT_00fe1dd8);
    FUN_00758620(uVar15,uVar17,uVar14,0);
    break;
  case 0x48:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar14 = FUN_00644250(&DAT_00fe1dd8);
    uVar15 = FUN_006442ec(&DAT_00fe1dd8);
    uVar17 = FUN_006442ec(&DAT_00fe1dd8);
    FUN_007585b8(uVar15,uVar17,uVar14,0);
    break;
  case 0x49:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    uVar7 = FUN_00644250(&DAT_00fe1dd8);
    uVar17 = (&DAT_0107034c)[uVar7 & 0xf];
    uVar14 = FUN_00644278(&DAT_00fe1dd8);
    uVar15 = FUN_00644278(&DAT_00fe1dd8);
    local_64c = VectorSignedToFloat(uVar14,(byte)(in_fpscr >> 0x16) & 3);
    local_648 = VectorSignedToFloat(uVar15,(byte)(in_fpscr >> 0x16) & 3);
    FUN_006eecc4(uVar17,&local_64c,(int)uVar7 >> 4);
    break;
  case 0x51:
    if (DAT_00fe1ec4 == 2) {
      FUN_00644d18(uVar7);
    }
    iVar8 = FUN_00644250(&DAT_00fe1dd8);
    iVar8 = (&DAT_0107034c)[iVar8];
    uVar14 = FUN_00644250(&DAT_00fe1dd8);
    *(undefined4 *)(iVar8 + 0x5bd8) = uVar14;
    break;
  case 0x52:
    uVar9 = FUN_00644250(&DAT_00fe1dd8);
    local_658 = (undefined1 *)((int)uVar9 >> 4);
    local_13c = (undefined *)(uVar9 & 0xf);
    uVar32 = FUN_006442ec(&DAT_00fe1dd8);
    local_14c = (undefined1 *)FUN_00644314(&DAT_00fe1dd8);
    puVar33 = &DAT_0107af40;
    iVar8 = 0;
    puVar21 = puVar33;
    do {
      if ((((undefined *)(uint)*(byte *)(puVar21 + 0x11) == (undefined *)(uVar9 & 0xf)) &&
          ((undefined1 *)(uint)(ushort)puVar21[1] == local_14c)) &&
         (*(char *)(puVar21 + 2) == '\x01')) break;
      iVar8 = iVar8 + 1;
      puVar21 = puVar21 + 0x74;
    } while (iVar8 < 0x200);
    iVar8 = 0;
    do {
      iVar16 = iVar8;
      if (*(char *)(puVar33 + 2) == '\0') break;
      iVar8 = iVar8 + 1;
      puVar33 = puVar33 + 0x74;
      iVar16 = 0x200;
    } while (iVar8 < 0x200);
    iVar8 = iVar16 * 0xe8;
    if (((&DAT_0107af44)[iVar8] == '\0') ||
       (puVar31 = &DAT_0107af40 + iVar16 * 0x74, *puVar31 != uVar32)) {
      puVar31 = &DAT_0107af40 + iVar16 * 0x74;
      FUN_006f0e64(puVar31);
    }
    *puVar31 = (ushort)uVar32;
    (&DAT_0107af62)[iVar8] = (char)local_13c;
    (&DAT_0107af42)[iVar16 * 0x74] = (short)local_14c;
    puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_52c);
    (&DAT_0107af84)[iVar16 * 0x3a] = *puVar10;
    (&DAT_0107af88)[iVar16 * 0x3a] = puVar10[1];
    *(int *)(&DAT_0107af6c + iVar8) = (int)(float)(&DAT_0107af84)[iVar16 * 0x3a];
    *(int *)(&DAT_0107af70 + iVar8) = (int)(float)(&DAT_0107af88)[iVar16 * 0x3a];
    puVar10 = (undefined4 *)FUN_006451b4(&DAT_00fe1dd8,auStack_534);
    puVar11 = local_658;
    *(undefined4 *)(&DAT_0107af94 + iVar16 * 0xe8) = *puVar10;
    *(undefined4 *)(&DAT_0107af98 + iVar16 * 0xe8) = puVar10[1];
    if (((uint)local_658 & 1) == 0) {
      *(undefined1 **)(&DAT_0107af7c + iVar8) = puVar19;
    }
    else {
      (**(code **)(DAT_00fe1dd8 + 0x1c))(&DAT_00fe1dd8,&local_14c,4);
      *(undefined1 **)(&DAT_0107af7c + iVar8) = local_14c;
    }
    if (((uint)puVar11 & 2) == 0) {
      sVar6 = 0;
    }
    else {
      (**(code **)(DAT_00fe1dd8 + 0x1c))(&DAT_00fe1dd8,local_148,2);
      sVar6 = local_148[0];
    }
    *(short *)(&DAT_0107afc2 + iVar8) = sVar6;
    if (((uint)puVar11 & 4) != 0) {
      (**(code **)(DAT_00fe1dd8 + 0x1c))(&DAT_00fe1dd8,&local_14c,4);
      puVar19 = local_14c;
    }
    (&DAT_0107afb4)[iVar16 * 0x3a] = puVar19;
    if (((uint)puVar11 & 8) == 0) {
      iVar8 = 0;
    }
    else {
      (**(code **)(DAT_00fe1dd8 + 0x1c))(&DAT_00fe1dd8,local_148,2);
      iVar8 = (int)local_148[0];
    }
    (&DAT_0107afb8)[iVar16 * 0x3a] = iVar8;
    if (DAT_00fe1ec4 == 2) {
      FUN_00644c88(uVar7,puVar31);
    }
  }
  FUN_007ffb98();
  return;
}


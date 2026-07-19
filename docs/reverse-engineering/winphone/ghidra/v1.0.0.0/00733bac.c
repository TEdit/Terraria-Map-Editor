
void FUN_00733bac(void)

{
  int *piVar1;
  int iVar2;
  byte bVar3;
  uint uVar4;
  byte *pbVar5;
  uint uVar6;
  short sVar7;
  uint in_fpscr;
  float fVar8;
  float fVar9;
  longlong lVar10;
  uint local_3c;
  byte local_38 [2];
  short local_36;
  char local_34 [2];
  undefined2 local_32;
  byte local_30;
  byte local_2f [11];
  
  lVar10 = FUN_007ffb80();
  piVar1 = (int *)lVar10;
  if (0 < DAT_00902ea0) {
    local_3c = 0;
    do {
      if ((local_3c & 0x1f) == 0) {
        iVar2 = FUN_0058ef48();
        fVar9 = (float)VectorSignedToFloat(local_3c,(byte)(in_fpscr >> 0x16) & 3);
        fVar8 = (float)VectorSignedToFloat((int)DAT_00902ea0,(byte)(in_fpscr >> 0x16) & 3);
        fVar9 = fVar9 / fVar8;
        uVar4 = in_fpscr & 0xfffffff | (uint)(fVar9 < 1.0) << 0x1f | (uint)(fVar9 == 1.0) << 0x1e;
        in_fpscr = uVar4 | (uint)NAN(fVar9) << 0x1c;
        *(float *)(iVar2 + 0x2c10) = fVar9;
        bVar3 = (byte)(uVar4 >> 0x18);
        if (!(bool)(bVar3 >> 6 & 1) && bVar3 >> 7 == ((byte)(in_fpscr >> 0x1c) & 1)) {
          *(float *)(iVar2 + 0x2c10) = 1.0;
        }
      }
      sVar7 = 0;
      pbVar5 = (byte *)(local_3c * DAT_00902934 * 0xe + DAT_00902928);
      if (0 < DAT_00902ea4) {
        do {
          *pbVar5 = 0;
          (**(code **)(*piVar1 + 0x1c))(piVar1,local_2f,1);
          bVar3 = (pbVar5[1] ^ local_2f[0]) & 1 ^ pbVar5[1];
          pbVar5[1] = bVar3;
          if ((bVar3 & 1) != 0) {
            (**(code **)(*piVar1 + 0x1c))(piVar1,local_38,1);
            uVar6 = (uint)local_38[0];
            uVar4 = uVar6;
            if (lVar10 < 0x3a00000000) {
              if ((uVar6 == 0x23) || (uVar6 == 0x24)) {
                uVar4 = 0x22;
              }
              else if (uVar6 == 0x96) {
                uVar4 = 500;
              }
            }
            *(short *)(pbVar5 + 6) = (short)uVar4;
            if (uVar4 == 0x7f) {
              pbVar5[1] = pbVar5[1] & 0xfe;
            }
            iVar2 = FUN_0072ad80(uVar6);
            if (iVar2 == 0) {
              if (((&DAT_010262d0)[uVar4 * 4] & 0x10000) == 0) {
                pbVar5[10] = 0xff;
                pbVar5[0xb] = 0xff;
                pbVar5[0xc] = 0xff;
                pbVar5[0xd] = 0xff;
              }
              else {
                pbVar5[10] = 0;
                pbVar5[0xb] = 0;
                pbVar5[0xc] = 0;
                pbVar5[0xd] = 0;
              }
            }
            else {
              (**(code **)(*piVar1 + 0x1c))(piVar1,&local_32,2);
              *(undefined2 *)(pbVar5 + 10) = local_32;
              (**(code **)(*piVar1 + 0x1c))(piVar1,&local_32,2);
              *(undefined2 *)(pbVar5 + 0xc) = local_32;
              if (*(short *)(pbVar5 + 6) == 0x90) {
                pbVar5[0xc] = 0;
                pbVar5[0xd] = 0;
              }
            }
            FUN_0072b00c(pbVar5,(int)((ulonglong)lVar10 >> 0x20));
            if (*(short *)(pbVar5 + 6) == 0x13) {
              if (*(short *)(pbVar5 + 0xc) < 0) {
                pbVar5[0xc] = 0;
                pbVar5[0xd] = 0;
              }
            }
            else if (*(short *)(pbVar5 + 6) == 0x90) {
              pbVar5[0xc] = 0;
              pbVar5[0xd] = 0;
            }
            else if (lVar10 < 0x3a00000000) {
              if (uVar6 == 0x23) {
                *(short *)(pbVar5 + 0xc) = *(short *)(pbVar5 + 0xc) + 0x36;
              }
              else if (uVar6 == 0x24) {
                *(short *)(pbVar5 + 0xc) = *(short *)(pbVar5 + 0xc) + 0x6c;
              }
            }
          }
          (**(code **)(*piVar1 + 0x1c))(piVar1,&local_30,1);
          pbVar5[8] = local_30;
          if (0x32ffffffff < lVar10) {
            (**(code **)(*piVar1 + 0x1c))(piVar1,local_38,1);
            pbVar5[5] = local_38[0];
          }
          (**(code **)(*piVar1 + 0x1c))(piVar1,local_38,1);
          pbVar5[4] = local_38[0];
          if (local_38[0] != 0) {
            (**(code **)(*piVar1 + 0x1c))(piVar1,local_34,1);
            *(ushort *)(pbVar5 + 2) =
                 *(ushort *)(pbVar5 + 2) ^
                 (*(ushort *)(pbVar5 + 2) ^ (ushort)(local_34[0] != '\0') << 0xc) & 0x3000;
          }
          (**(code **)(*piVar1 + 0x1c))(piVar1,local_38,1);
          *pbVar5 = *pbVar5 | local_38[0];
          iVar2 = FUN_007a7d34();
          if (iVar2 != 0) {
            *pbVar5 = *pbVar5 & 0xf7;
          }
          local_36 = 0;
          (**(code **)(*piVar1 + 0x1c))(piVar1,&local_36,2);
          sVar7 = sVar7 + local_36;
          for (; 0 < local_36; local_36 = local_36 + -1) {
            *(undefined4 *)(pbVar5 + 0xe) = *(undefined4 *)pbVar5;
            *(undefined4 *)(pbVar5 + 0x12) = *(undefined4 *)(pbVar5 + 4);
            *(undefined4 *)(pbVar5 + 0x16) = *(undefined4 *)(pbVar5 + 8);
            *(undefined2 *)(pbVar5 + 0x1a) = *(undefined2 *)(pbVar5 + 0xc);
            pbVar5 = pbVar5 + 0xe;
          }
          sVar7 = sVar7 + 1;
          pbVar5 = pbVar5 + 0xe;
        } while (sVar7 < DAT_00902ea4);
      }
      local_3c = (uint)(short)((short)local_3c + 1);
    } while ((int)local_3c < (int)DAT_00902ea0);
  }
  FUN_007ffb98();
  return;
}


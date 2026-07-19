
void FUN_006921b0(int param_1,int param_2,undefined4 param_3,undefined4 param_4)

{
  float fVar1;
  undefined1 uVar2;
  int iVar3;
  int iVar4;
  int iVar5;
  uint in_fpscr;
  float fVar6;
  undefined4 uVar7;
  undefined4 uVar8;
  
  uVar7 = 0xfffffffe;
  uVar8 = 0xfffffffe;
  iVar3 = FUN_00735520();
  if (((iVar3 == 0) || (DAT_00fe1f86 != '\0')) || (DAT_010703e6 < 6)) {
    iVar5 = 0;
    iVar3 = DAT_00fe1fa8;
    do {
      if (*(char *)(iVar3 + 0x100) == '\0') {
        if (0xc3 < iVar5) {
          return;
        }
        FUN_00684770(iVar5 * 0x274 + DAT_00fe1fa8,0x2e,iVar3,0,uVar7,uVar8,param_4);
        iVar3 = iVar5 * 0x274 + DAT_00fe1fa8;
        param_1 = param_1 - (uint)(*(ushort *)(iVar3 + 0x168) >> 1);
        *(int *)(iVar3 + 0x148) = param_1;
        uVar7 = VectorSignedToFloat(param_1,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar3 + 0x138) = uVar7;
        iVar3 = iVar5 * 0x274 + DAT_00fe1fa8;
        param_2 = param_2 - (uint)*(ushort *)(iVar3 + 0x16a);
        *(int *)(iVar3 + 0x14c) = param_2;
        uVar7 = VectorSignedToFloat(param_2,(byte)(in_fpscr >> 0x16) & 3);
        *(undefined4 *)(iVar3 + 0x13c) = uVar7;
        *(undefined1 *)(iVar5 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
        *(undefined4 *)(iVar5 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
        iVar3 = iVar5 * 0x274 + DAT_00fe1fa8;
        uVar2 = FUN_0060a19c(iVar3 + 0x138,*(undefined2 *)(iVar3 + 0x168),
                             *(undefined2 *)(iVar3 + 0x16a));
        *(undefined1 *)(iVar5 * 0x274 + DAT_00fe1fa8 + 100) = uVar2;
        FUN_00649e08(iVar5,1);
        return;
      }
      iVar5 = iVar5 + 1;
      iVar3 = iVar3 + 0x274;
    } while (iVar5 < 0xc4);
  }
  else {
    iVar5 = 0xc4;
    iVar4 = 0;
    iVar3 = DAT_00fe1fa8;
    do {
      if (*(char *)(iVar3 + 0x100) == '\0') {
        iVar5 = iVar4;
        if (iVar4 < 0xc4) {
          FUN_00684770(iVar4 * 0x274 + DAT_00fe1fa8,0x400,iVar3,0,uVar7,uVar8,param_4);
          iVar3 = iVar4 * 0x274 + DAT_00fe1fa8;
          param_1 = param_1 - (uint)(*(ushort *)(iVar3 + 0x168) >> 1);
          *(int *)(iVar3 + 0x148) = param_1;
          uVar7 = VectorSignedToFloat(param_1,(byte)(in_fpscr >> 0x16) & 3);
          *(undefined4 *)(iVar3 + 0x138) = uVar7;
          iVar3 = iVar4 * 0x274 + DAT_00fe1fa8;
          param_2 = param_2 - (uint)*(ushort *)(iVar3 + 0x16a);
          *(int *)(iVar3 + 0x14c) = param_2;
          uVar7 = VectorSignedToFloat(param_2,(byte)(in_fpscr >> 0x16) & 3);
          *(undefined4 *)(iVar3 + 0x13c) = uVar7;
          *(undefined1 *)(iVar4 * 0x274 + DAT_00fe1fa8 + 0x100) = 1;
          *(undefined4 *)(iVar4 * 0x274 + DAT_00fe1fa8 + 0x198) = 0x2ee;
          iVar3 = iVar4 * 0x274 + DAT_00fe1fa8;
          uVar2 = FUN_0060a19c(iVar3 + 0x138,*(undefined2 *)(iVar3 + 0x168),
                               *(undefined2 *)(iVar3 + 0x16a));
          *(undefined1 *)(iVar4 * 0x274 + DAT_00fe1fa8 + 100) = uVar2;
          FUN_00649e08(iVar4,1);
        }
        break;
      }
      iVar4 = iVar4 + 1;
      iVar3 = iVar3 + 0x274;
    } while (iVar4 < 0xc4);
    fVar1 = DAT_006923dc;
    iVar3 = iVar5 * 0x274 + DAT_00fe1fa8;
    fVar6 = (float)VectorSignedToFloat(*(undefined4 *)(iVar3 + 0x19c),(byte)(in_fpscr >> 0x16) & 3);
    *(int *)(iVar3 + 0x19c) = (int)(fVar6 * DAT_006923dc);
    iVar3 = iVar5 * 0x274 + DAT_00fe1fa8;
    fVar6 = (float)VectorSignedToFloat(*(undefined4 *)(iVar3 + 0x1b8),(byte)(in_fpscr >> 0x16) & 3);
    *(int *)(iVar3 + 0x1b8) = (int)(fVar6 * fVar1);
    iVar3 = iVar5 * 0x274 + DAT_00fe1fa8;
    fVar6 = (float)VectorSignedToFloat(*(undefined4 *)(iVar3 + 0x1b4),(byte)(in_fpscr >> 0x16) & 3);
    *(int *)(iVar3 + 0x1b4) = (int)(fVar6 * fVar1);
  }
  return;
}


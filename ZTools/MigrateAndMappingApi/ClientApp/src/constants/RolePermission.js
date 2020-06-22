import { EnumAction, EnumMethod } from './../enums/Enums'
const Methods = [
    {
        id: EnumMethod.GET,
        name: 'Get'
    },
    {
        id: EnumMethod.POST,
        name: 'Post'
    },
    {
        id: EnumMethod.PUT,
        name: 'Put'
    },
    {
        id: EnumMethod.DELETE,
        name: 'Delete'
    },
    {
        id: EnumMethod.PATCH,
        name: 'Patch'
    }
];


const Actions = [
    {
        id: EnumAction.VIEW,
        name: 'View'
    },
    {
        id: EnumAction.ADD,
        name: 'Add'
    },
    {
        id: EnumAction.UPDATE,
        name: 'Update'
    },
    {
        id: EnumAction.DELETE,
        name: 'Delete'
    },
    {
        id: EnumAction.CENSOR,
        name: 'Censor'
    },
    {
        id: EnumAction.CHECK,
        name: 'Check'
    }
];

export { Methods, Actions };


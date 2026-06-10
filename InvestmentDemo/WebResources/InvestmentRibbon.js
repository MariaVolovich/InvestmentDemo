var InvestmentRibbon = (function () {
    debugger;

    function confirmInvestment(primaryControl) {

        var formContext = primaryControl;

        Xrm.Navigation.openConfirmDialog({
            title: "Confirm Investment",
            text: "Confirm this investment?"
        }).then(function (result) {

            if (!result.confirmed)
                return;

            if (formContext.data.entity.getIsDirty()) {

                Xrm.Navigation.openAlertDialog({
                    text: "You have unsaved changes. Save the record before turning it to Confirmed."
                });

                return;
            }

                var recordId = formContext.data.entity.getId();

                if (!recordId) {
                    Xrm.Navigation.openAlertDialog({
                        text: "Please save the record first."
                    });
                    return;
                }

                recordId = recordId.replace("{", "").replace("}", "");

                var request = {
                    entity: {
                        entityType: "mv_investment",
                        id: recordId
                    },

                    getMetadata: function () {
                        return {
                            boundParameter: "entity",
                            parameterTypes: {
                                entity: {
                                    typeName: "mscrm.mv_investment",
                                    structuralProperty: 5
                                }
                            },
                            operationType: 0,
                            operationName: "mv_ConfirmInvestment"
                        };
                    }
                };

                Xrm.WebApi.online.execute(request).then(
                    function success(result) {

                        if (result.ok) {

                            formContext.data.refresh(false).then(() => formContext.ui.refreshRibbon());

                        }

                    },
                    function (error) {

                        Xrm.Navigation.openAlertDialog({
                            text: error.message
                        });

                    }
                );

        });

    }
   
        function returnToDraft(primaryControl) {
            debugger;
            var formContext = primaryControl;

            Xrm.Navigation.openConfirmDialog({
                title: "Return to Draft",
                text: "Move this investment back to Draft?"
            }).then(function (result) {

                if (!result.confirmed)
                    return;

                if (formContext.data.entity.getIsDirty()) {

                    Xrm.Navigation.openAlertDialog({
                        text: "You have unsaved changes. Save the record before returning it to Draft."
                    });

                    return;
                }


                var recordId = formContext.data.entity.getId();

                if (!recordId) {
                    Xrm.Navigation.openAlertDialog({
                        text: "Please save the record first."
                    });
                    return;
                }

                recordId = recordId.replace("{", "").replace("}", "");

                var request = {
                    entity: {
                        entityType: "mv_investment",
                        id: recordId
                    },

                    getMetadata: function () {
                        return {
                            boundParameter: "entity",
                            parameterTypes: {
                                entity: {
                                    typeName: "mscrm.mv_investment",
                                    structuralProperty: 5
                                }
                            },
                            operationType: 0,
                            operationName: "mv_ReturnInvestmentToDraft"
                        };
                    }
                };

                Xrm.WebApi.online.execute(request).then(
                    function success(result) {

                        if (result.ok) {

                            formContext.data.refresh(false).then(() => formContext.ui.refreshRibbon());

                        }

                    },
                    function (error) {

                        Xrm.Navigation.openAlertDialog({
                            text: error.message
                        });

                    }
                );                   
            });
        }       
    

    return {
        confirmInvestment: confirmInvestment,
        returnToDraft: returnToDraft
    };

})();
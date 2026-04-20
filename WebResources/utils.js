console.log("utils.js v9");
var CRM = CRM || {};

CRM.Utils = (function () {
    function getAttribute(formContext, fieldName) {
        return formContext.getAttribute(fieldName);
    }

    function getValue(formContext, fieldName) {
        var attr = getAttribute(formContext, fieldName);
        return attr ? attr.getValue() : null;
    }

    function setValue(formContext, fieldName, value) {
        var attr = getAttribute(formContext, fieldName);

        if (attr) {
            attr.setValue(value);
        }
    }

    function setLookupValue(formContext, fieldName, lookup) {
        var attr = getAttribute(formContext, fieldName);

        if (attr) {
            attr.setValue([lookup]);
        }
    }

    function showNotification(formContext, message, level, id) {
        formContext.ui.setFormNotification(message, level, id);
    }

    function clearNotification(formContext, id) {
        formContext.ui.clearFormNotification(id);
    }

    function setVisible(formContext, fieldName, visible) {
        var control = formContext.getControl(fieldName);

        if (control) {
            control.setVisible(visible);
        }
    }

    function setDisabled(formContext, fieldName, disabled) {
        var control = formContext.getControl(fieldName);

        if (control) {
            control.setDisabled(disabled);
        }
    }

    function log(message, data) {
        console.log("[CRM]", message, data || "");
    }

    function retrieveMultiple(entityName, query) {

        return Xrm.WebApi.retrieveMultipleRecords(entityName, query)
            .then(function (result) {
                return result.entities;
            })
            .catch(function (error) {
                console.error("[CRM][retrieveMultiple]", error.message);
                return [];
            });
    }

    function retrieveById(entityName, id, selectFields) {

        if (!id)
            return Promise.resolve(null);

        // handle lookup array input automatically
        if (Array.isArray(id)) {
            id = id[0]?.id;
        }

        // handle lookup object input
        if (typeof id === "object" && id.id) {
            id = id.id;
        }

        // now ensure it's string
        if (typeof id !== "string") {
            console.warn("[CRM][retrieveById] Invalid ID format:", id);
            return Promise.resolve(null);
        }

        var cleanId = id.replace(/[{}]/g, "");

        var query = selectFields && selectFields.length
            ? `?$select=${selectFields.join(",")}`
            : "";

        return Xrm.WebApi.retrieveRecord(entityName, cleanId, query)
            .catch(function (error) {
                console.error("[CRM][retrieveById]", error.message);
                return null;
            });
    }


    return {
        getAttribute: getAttribute,
        getValue: getValue,
        setValue: setValue,
        setLookupValue: setLookupValue,
        showNotification: showNotification,
        clearNotification: clearNotification,
        setVisible: setVisible,
        setDisabled: setDisabled,
        log: log,
        retrieveMultiple: retrieveMultiple,
        retrieveById: retrieveById
    };

})();